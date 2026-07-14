using BingoCompany.Api.Hubs;
using BingoCompany.Api.Interfaces;
using BingoCompany.Api.Security;
using BingoCompany.Api.Contracts;
using BingoCompany.Api.Services;
using BingoCompany.Application;
using BingoCompany.Application.Services;
using BingoCompany.Application.Interfaces;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BingoCompany.Domain;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/events"), Authorize, ServiceFilter<CompanyEventOwnerFilter>]
public sealed class EventsController(BingoDbContext db, IHubContext<BingoHub> hub, IBingoRoundGameplayService gameplayService, IEventParticipantRegistrationService participantRegistrationService) : ControllerBase
{
	private const int MaximumPrizeImageLength = 2_800_000;
	private const int DefaultEventsPageSize = 12;
	private const int MaximumEventsPageSize = 100;
	private static readonly string[] SupportedPrizeImagePrefixes = ["data:image/jpeg;base64,", "data:image/png;base64,", "data:image/webp;base64,"];
	[HttpGet]
	public async Task<ActionResult<object>> List([FromQuery] int page = 1, [FromQuery] int pageSize = DefaultEventsPageSize)
	{
		var normalizedPage = Math.Max(page, 1);
		var normalizedPageSize = Math.Clamp(pageSize, 1, MaximumEventsPageSize);
		var events = db.Events
			.AsNoTracking()
			.Where(item => item.CompanyId == GetCompanyId())
			.OrderByDescending(item => item.CreatedAt)
			.ThenByDescending(item => item.Id);
		var totalItems = await events.CountAsync(HttpContext.RequestAborted);
		var items = await events
			.Skip((normalizedPage - 1) * normalizedPageSize)
			.Take(normalizedPageSize)
			.Select(item => new { item.Id, item.Name, item.PublicCode, item.Status, item.MarkingMode, item.CreatedAt })
			.ToListAsync(HttpContext.RequestAborted);
		return Ok(new { items, page = normalizedPage, pageSize = normalizedPageSize, totalItems, totalPages = (int)Math.Ceiling(totalItems / (double)normalizedPageSize) });
	}
	[HttpPost]
	public async Task<ActionResult<object>> Create(CreateEventRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Informe o nome do evento.");
		var bingoEvent = new BingoEvent(GetCompanyId(), request.Name, request.CardsPerParticipant <= 0 ? 1 : request.CardsPerParticipant, request.MarkingMode);
		db.Events.Add(bingoEvent); db.AuditEntries.Add(new AuditEntry(bingoEvent.Id, "Evento criado", $"Evento {bingoEvent.Name} criado.")); await db.SaveChangesAsync();
		return CreatedAtAction(nameof(Get), new { eventId = bingoEvent.Id }, new { bingoEvent.Id, bingoEvent.Name, bingoEvent.PublicCode, bingoEvent.Status, bingoEvent.MarkingMode, bingoEvent.CreatedAt });
	}
	[HttpGet("{eventId:guid}")]
	public async Task<ActionResult<object>> Get(Guid eventId)
	{
		var e = await db.Events.Include(x => x.Rounds).ThenInclude(x => x.Stages).Include(x => x.Cards).Include(x => x.Participants).SingleOrDefaultAsync(x => x.Id == eventId);
		return e is null ? NotFound() : Ok(new { e.Id, e.Name, e.PublicCode, e.Status, participants = e.Participants.Count, cards = e.Cards.Count, participantList = e.Participants.OrderBy(item => item.Name).Select(item => new { item.Id, item.Name, item.Type }), cardList = e.Cards.OrderByDescending(item => item.CreatedAt).Select(item => new { item.PublicCode, item.Type, item.Status, item.Fingerprint, item.ParticipantId }), rounds = e.Rounds.OrderBy(x => x.Sequence).Select(x => new { x.Id, x.Name, x.Status, stages = x.Stages.OrderBy(stage => stage.Sequence).Select(stage => new { stage.Sequence, stage.PrizeName, stage.Pattern, stage.PrizeImageDataUrl, stage.IsActive, stage.IsCompleted }) }) });
	}
	[HttpPost("{eventId:guid}/registration/open")]
	public async Task<IActionResult> OpenRegistration(Guid eventId) { var e = await db.Events.FindAsync(eventId); if (e is null) return NotFound(); e.OpenRegistration(); db.AuditEntries.Add(new AuditEntry(eventId, "Inscrições abertas", "As inscrições do evento foram abertas.")); await db.SaveChangesAsync(); return NoContent(); }
	[HttpPost("{eventId:guid}/participants")]
	public async Task<ActionResult<object>> Join(Guid eventId, JoinEventRequest request)
	{
		try
		{
			var registration = await participantRegistrationService.Register(eventId, request, HttpContext.RequestAborted);
			return registration is null
				? NotFound()
				: Ok(new { participantId = registration.ParticipantId, cardId = registration.CardId, registration.PublicCode, registration.Numbers });
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}
	[HttpPost("{eventId:guid}/cards/{cardCode}/next")]
	public async Task<ActionResult<object>> GenerateNextCard(Guid eventId, string cardCode)
	{
		var card = await db.Cards.SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode);
		if (card is null) return NotFound();
		if (card.ReplacementCardId.HasValue) return Conflict("Uma nova cartela já foi gerada a partir desta cartela.");

		var previousRound = await db.Rounds
			.Where(item => item.EventId == eventId && item.Status == RoundStatus.Finished)
			.OrderByDescending(item => item.Sequence)
			.FirstOrDefaultAsync();
		if (previousRound is null) return Conflict("A cartela poderá ser renovada após o encerramento de uma rodada.");
		if (await db.Rounds.AnyAsync(item => item.EventId == eventId && item.Sequence > previousRound.Sequence && (item.Status == RoundStatus.Drawing || item.Status == RoundStatus.WinnerDetected || item.Status == RoundStatus.TieBreaker))) return Conflict("A próxima rodada já foi iniciada.");
		if (!await db.RoundEligibleCards.AnyAsync(item => item.RoundId == previousRound.Id && item.CardId == card.Id)) return Conflict("Esta cartela não participou da última rodada concluída.");

		var markedNumbers = await GetMarkedNumbers(eventId, card.Id, previousRound.Id);
		if (!card.IsComplete(markedNumbers)) return Conflict("A cartela anterior ainda não está completa.");

		var nextCard = new BingoCard(eventId, card.ParticipantId, CardType.Digital, new Bingo75CardGenerator().Generate());
		card.ReplaceWith(nextCard);
		db.Cards.Add(nextCard);
		await db.SaveChangesAsync();
		return Ok(new { cardId = nextCard.Id, nextCard.PublicCode, numbers = ToRows(nextCard.Numbers) });
	}
	[HttpPost("{eventId:guid}/cards/printed")]
	public async Task<ActionResult<object>> GeneratePrintedCards(Guid eventId, GeneratePrintedCardsRequest request)
	{
		var bingoEvent = await db.Events.FindAsync(eventId);
		if (bingoEvent is null) return NotFound();
		if (bingoEvent.Status == EventStatus.Finished) return Conflict("O evento encerrado não pode ser alterado.");
		if (request.Quantity is < 1 or > 1000) return BadRequest("Informe entre 1 e 1000 cartelas.");
		var cards = Enumerable.Range(0, request.Quantity).Select(_ => new BingoCard(eventId, null, CardType.Printed, new Bingo75CardGenerator().Generate())).ToArray();
		db.Cards.AddRange(cards);
		db.AuditEntries.Add(new AuditEntry(eventId, "Cartelas impressas geradas", $"Lote com {cards.Length} cartelas impressas gerado."));
		await db.SaveChangesAsync();
		return Ok(cards.Select(card => new { card.PublicCode, card.Fingerprint, qrCodeValue = $"/cartelas/{card.PublicCode}" }));
	}
	[HttpGet("{eventId:guid}/cards/printed")]
	public async Task<ActionResult<object>> GetPrintedCards(Guid eventId)
	{
		var companyName = await db.Events
			.Where(item => item.Id == eventId)
			.Join(db.Companies, bingoEvent => bingoEvent.CompanyId, company => company.Id, (_, company) => company.Name)
			.SingleOrDefaultAsync();
		if (companyName is null) return NotFound();

		var cards = await db.Cards.Where(item => item.EventId == eventId && item.Type == CardType.Printed).OrderBy(item => item.CreatedAt).ToListAsync();
		return Ok(cards.Select(card => new { card.PublicCode, card.Fingerprint, card.Status, companyName, numbers = ToRows(card.Numbers), qrCodeValue = $"BINGO:{eventId:N}:{card.PublicCode}:{card.Fingerprint}" }));
	}
	[HttpPost("{eventId:guid}/cards/{cardCode}/assign")]
	public async Task<IActionResult> AssignPrintedCard(Guid eventId, string cardCode, AssignPrintedCardRequest request)
	{
		var card = await db.Cards.SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode);
		var participant = await db.Participants.SingleOrDefaultAsync(item => item.Id == request.ParticipantId && item.EventId == eventId);
		if (card is null || participant is null) return NotFound();
		if (card.Type != CardType.Printed) return BadRequest("Apenas cartelas impressas podem ser associadas por esta operação.");
		card.Assign(participant.Id);
		db.AuditEntries.Add(new AuditEntry(eventId, "Cartela associada", $"Cartela {card.PublicCode} associada a participante."));
		await db.SaveChangesAsync();
		return NoContent();
	}
	[HttpPost("{eventId:guid}/cards/{cardCode}/activate")]
	public async Task<IActionResult> ActivatePrintedCard(Guid eventId, string cardCode)
	{
		var card = await db.Cards.SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode);
		if (card is null) return NotFound();
		if (card.Type != CardType.Printed) return BadRequest("Apenas cartelas impressas podem ser ativadas por esta operação.");
		card.Activate();
		db.AuditEntries.Add(new AuditEntry(eventId, "Cartela ativada", $"Cartela {card.PublicCode} ativada."));
		await db.SaveChangesAsync();
		return NoContent();
	}
	[HttpPost("{eventId:guid}/rounds")]
	public async Task<ActionResult<object>> CreateRound(Guid eventId, CreateRoundRequest request)
	{
		var bingoEvent = await db.Events.FindAsync(eventId);
		if (bingoEvent is null) return NotFound();
		if (bingoEvent.Status == EventStatus.Finished) return Conflict("O evento encerrado não pode ser alterado.");
		if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Informe o nome da rodada.");
		if (request.Stages.Count == 0) return BadRequest("Configure ao menos uma etapa de prêmio.");
		if (request.Stages.Any(stage => string.IsNullOrWhiteSpace(stage.PrizeName))) return BadRequest("Informe o nome de cada prêmio.");
		if (request.Stages.Any(stage => !IsValidPrizeImage(stage.PrizeImageDataUrl))) return BadRequest("A foto do prêmio deve ser uma imagem JPEG, PNG ou WebP de até 2 MB.");
		if (HasDuplicatePatterns(request.Stages)) return BadRequest("Cada regra de premiação pode ser usada apenas uma vez por rodada.");
		var count = await db.Rounds.CountAsync(x => x.EventId == eventId); var round = new BingoRound(eventId, count + 1, request.Name);
		var orderedStages = PrizeStageOrdering.Order(request.Stages, stage => stage.Pattern);
		for (var index = 0; index < orderedStages.Count; index++)
		{
			var stage = orderedStages[index];
			round.AddStage(new PrizeStage(round.Id, index + 1, stage.PrizeName, stage.Pattern, stage.PrizeImageDataUrl));
		}
		db.Rounds.Add(round); db.AuditEntries.Add(new AuditEntry(eventId, "Rodada criada", $"Rodada {round.Name} criada com {request.Stages.Count} etapas.")); await db.SaveChangesAsync(); return Ok(new { round.Id, round.Name });
	}
	[HttpPut("{eventId:guid}/rounds/{roundId:guid}")]
	public async Task<IActionResult> UpdateRound(Guid eventId, Guid roundId, CreateRoundRequest request)
	{
		var bingoEvent = await db.Events.FindAsync(eventId);
		var round = await db.Rounds.Include(item => item.Stages).SingleOrDefaultAsync(item => item.Id == roundId && item.EventId == eventId);
		if (bingoEvent is null || round is null) return NotFound();
		if (bingoEvent.Status == EventStatus.Finished) return Conflict("O evento encerrado não pode ser alterado.");
		if (round.Status != RoundStatus.Ready) return Conflict("A rodada só pode ser editada antes do início do sorteio.");
		if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Informe o nome da rodada.");
		if (request.Stages.Count == 0) return BadRequest("Configure ao menos uma etapa de prêmio.");
		if (request.Stages.Any(stage => string.IsNullOrWhiteSpace(stage.PrizeName))) return BadRequest("Informe o nome de cada prêmio.");
		if (request.Stages.Any(stage => !IsValidPrizeImage(stage.PrizeImageDataUrl))) return BadRequest("A foto do prêmio deve ser uma imagem JPEG, PNG ou WebP de até 2 MB.");
		if (HasDuplicatePatterns(request.Stages)) return BadRequest("Cada regra de premiação pode ser usada apenas uma vez por rodada.");

		var stages = PrizeStageOrdering.Order(request.Stages, stage => stage.Pattern)
			.Select((stage, index) => new PrizeStage(round.Id, index + 1, stage.PrizeName, stage.Pattern, stage.PrizeImageDataUrl))
			.ToArray();
		db.PrizeStages.RemoveRange(round.Stages);
		round.Update(request.Name, stages);
		db.AuditEntries.Add(new AuditEntry(eventId, "Rodada editada", $"Rodada {round.Name} atualizada com {stages.Length} etapas."));
		await db.SaveChangesAsync();
		return NoContent();
	}
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/start")]
	public async Task<IActionResult> StartRound(Guid eventId, Guid roundId)
	{
		var e = await db.Events.FindAsync(eventId); var round = await db.Rounds.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId); if (e is null || round is null) return NotFound();
		if (e.Status == EventStatus.Finished) return Conflict("O evento encerrado não pode ser alterado.");
		if (e.Status == EventStatus.RegistrationOpen) e.Start();
		round.FreezeEligibility(await db.Cards.Where(x => x.EventId == eventId).ToListAsync());
		db.RoundEligibleCards.AddRange(round.EligibleCards);
		var sequence = SecureDrawSequence.Generate();
		round.Start(sequence, SecureDrawSequence.Hash(sequence));
		db.AuditEntries.Add(new AuditEntry(eventId, "Rodada iniciada", $"Rodada {round.Name} iniciada. Hash {round.SequenceHash}."));
		await db.SaveChangesAsync();
		await hub.Clients.Group($"round:{roundId}").SendAsync("RoundStarted", new { roundId, round.SequenceHash }); return NoContent();
	}
	[HttpPost("{eventId:guid}/finish")]
	public async Task<IActionResult> FinishEvent(Guid eventId)
	{
		var bingoEvent = await db.Events.Include(item => item.Rounds).SingleOrDefaultAsync(item => item.Id == eventId);
		if (bingoEvent is null) return NotFound();
		bingoEvent.Finish();
		db.AuditEntries.Add(new AuditEntry(eventId, "Evento encerrado", "O evento foi encerrado e tornou-se imutável."));
		await db.SaveChangesAsync();
		await hub.Clients.Group($"event:{eventId}").SendAsync("EventFinished", new { eventId });
		return NoContent();
	}
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/draw")]
	public async Task<ActionResult<object>> Draw(Guid eventId, Guid roundId)
	{
		var e = await db.Events.FindAsync(eventId); var round = await db.Rounds.Include(x => x.Stages).Include(x => x.DrawnNumbers).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId); if (e is null || round is null) return NotFound();
		var eligibleCardIds = await db.RoundEligibleCards.Where(item => item.RoundId == roundId).Select(item => item.CardId).ToListAsync();
		var eligibleCards = await db.Cards.Where(item => eligibleCardIds.Contains(item.Id)).ToListAsync();
		var marks = e.MarkingMode == CardMarkingMode.Automatic ? [] : await db.CardMarks.Where(item => item.RoundId == roundId).ToListAsync();
		var drawResult = gameplayService.Draw(round, eligibleCards, marks, e.MarkingMode);
		db.DrawnNumbers.Add(drawResult.DrawnNumber);
		db.RoundWinners.AddRange(drawResult.Winners);
		var drawn = drawResult.DrawnNumber;
		db.AuditEntries.Add(new AuditEntry(eventId, "Pedra sorteada", $"Rodada {round.Name}: pedra {drawn.Number} na posição {drawn.Sequence}."));
		if (drawResult.HasWinners) db.AuditEntries.Add(new AuditEntry(eventId, "Prêmio detectado", $"{drawResult.Winners.Count} cartela(s) atingiram {round.ActiveStage.PrizeName}."));
		await db.SaveChangesAsync();
		var message = new { roundId, number = drawn.Number, sequence = drawn.Sequence, winnersDetected = drawResult.Winners.Count };
		await hub.Clients.Group($"round:{roundId}").SendAsync("NumberDrawn", message);
		await hub.Clients.Group($"event:{eventId}").SendAsync("NumberDrawn", message);
		if (drawResult.HasWinners)
		{
			var notice = new { roundId, count = drawResult.Winners.Count, tieBreakerRequired = drawResult.RequiresTieBreaker };
			await hub.Clients.Group($"round:{roundId}").SendAsync("WinnerDetected", notice);
			await hub.Clients.Group($"event:{eventId}").SendAsync("WinnerDetected", notice);
			await hub.Clients.Group($"round:{roundId}").SendAsync("WinningCardDetected", notice);
			await hub.Clients.Group($"event:{eventId}").SendAsync("WinningCardDetected", notice);
			if (drawResult.RequiresTieBreaker)
			{
				await hub.Clients.Group($"round:{roundId}").SendAsync("TieBreakerStarted", notice);
				await hub.Clients.Group($"event:{eventId}").SendAsync("TieBreakerStarted", notice);
			}
		}
		return Ok(message);
	}
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/reveal")]
	public async Task<ActionResult<object>> Reveal(Guid eventId, Guid roundId)
	{
		var round = await db.Rounds.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId); if (round is null || (round.Status != RoundStatus.WinnerDetected && round.Status != RoundStatus.TieBreaker)) return Conflict("Não há vencedor aguardando revelação.");
		var candidates = await db.RoundWinners.Where(x => x.RoundId == roundId && x.StageId == round.ActiveStage.Id).ToListAsync(); if (candidates.Count == 0) return NotFound();
		var revealResult = gameplayService.RevealWinner(round, candidates, DateTimeOffset.UtcNow);
		var winner = revealResult.Winner;
		db.AuditEntries.Add(new AuditEntry(eventId, candidates.Count > 1 ? "Desempate concluído" : "Prêmio revelado", $"Prêmio {round.Stages.Single(stage => stage.Id == winner.StageId).PrizeName} revelado."));
		if (round.Status == RoundStatus.Finished) db.AuditEntries.Add(new AuditEntry(eventId, "Rodada encerrada", $"Rodada {round.Name} encerrada."));
		await db.SaveChangesAsync();
		var participant = await db.Participants.FindAsync(winner.ParticipantId); var card = await db.Cards.FindAsync(winner.CardId); var result = new { participantName = participant!.Name, cardCode = card!.PublicCode, prize = candidates.Count > 0 ? (await db.PrizeStages.FindAsync(winner.StageId))!.PrizeName : "", tieBreakers = candidates.Select(x => new { participantName = db.Participants.Find(x.ParticipantId)!.Name, x.TieBreakerNumber, x.IsWinner }) };
		var stageChanged = new { roundId, currentPrize = round.Stages.SingleOrDefault(stage => stage.IsActive)?.PrizeName, status = round.Status };
		await hub.Clients.Group($"round:{roundId}").SendAsync("PrizeStageChanged", stageChanged);
		await hub.Clients.Group($"event:{eventId}").SendAsync("PrizeStageChanged", stageChanged);
		if (round.Status == RoundStatus.Finished)
		{
			await hub.Clients.Group($"round:{roundId}").SendAsync("RoundFinished", new { roundId });
			await hub.Clients.Group($"event:{eventId}").SendAsync("RoundFinished", new { roundId });
		}
		await hub.Clients.Group($"round:{roundId}").SendAsync("WinnerRevealed", result);
		await hub.Clients.Group($"event:{eventId}").SendAsync("WinnerRevealed", result); return Ok(result);
	}
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/winner-presentation/close")]
	public async Task<IActionResult> CloseWinnerPresentation(Guid eventId, Guid roundId)
	{
		var round = await db.Rounds.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId);
		if (round is null) return NotFound();

		round.CloseWinnerPresentation();
		await db.SaveChangesAsync();
		await hub.Clients.Group($"round:{roundId}").SendAsync("WinnerPresentationClosed", new { roundId });
		await hub.Clients.Group($"event:{eventId}").SendAsync("WinnerPresentationClosed", new { roundId });
		return NoContent();
	}
	[HttpPost("{eventId:guid}/cards/{cardCode}/marks")]
	public async Task<IActionResult> Mark(Guid eventId, string cardCode, MarkNumberRequest request)
	{
		var e = await db.Events.FindAsync(eventId); var card = await db.Cards.SingleOrDefaultAsync(x => x.EventId == eventId && x.PublicCode == cardCode);
		var round = await db.Rounds.Include(x => x.Stages).Include(x => x.DrawnNumbers).Where(x => x.EventId == eventId && x.Status == RoundStatus.Drawing).OrderByDescending(x => x.Sequence).FirstOrDefaultAsync();
		if (e is null || card is null || round is null) return NotFound();
		if (e.MarkingMode == CardMarkingMode.Automatic) return Conflict("Este evento usa marcação automática.");
		if (!card.IsEligible || !Enumerable.Range(0, 5).SelectMany(r => Enumerable.Range(0, 5).Select(c => card.Numbers[r, c])).Contains(request.Number)) return BadRequest("Número inválido para a cartela.");
		var drawn = round.DrawnNumbers.SingleOrDefault(x => x.Number == request.Number); if (drawn is null) return BadRequest("O número ainda não foi sorteado.");
		if (await db.CardMarks.AnyAsync(x => x.RoundId == round.Id && x.CardId == card.Id && x.Number == request.Number)) return NoContent();
		db.CardMarks.Add(new CardMark(round.Id, card.Id, request.Number, drawn.Sequence));
		var markedNumbers = (await db.CardMarks.Where(x => x.RoundId == round.Id && x.CardId == card.Id).Select(x => x.Number).ToListAsync()).Append(request.Number).ToHashSet();
		var winnerDetection = gameplayService.DetectManualWinner(round, card, markedNumbers, drawn.Sequence);
		if (winnerDetection.HasWinner) db.RoundWinners.Add(winnerDetection.Winner!);

		db.AuditEntries.Add(new AuditEntry(eventId, "Número marcado", $"Cartela {card.PublicCode} marcou a pedra {request.Number} na rodada {round.Name}."));
		if (winnerDetection.HasWinner) db.AuditEntries.Add(new AuditEntry(eventId, "Prêmio detectado", "Uma cartela manual atingiu a regra ativa."));
		await db.SaveChangesAsync();
		if (winnerDetection.HasWinner)
		{
			var notice = new { roundId = round.Id, count = 1 };
			await hub.Clients.Group($"round:{round.Id}").SendAsync("WinningCardDetected", notice);
			await hub.Clients.Group($"event:{eventId}").SendAsync("WinningCardDetected", notice);
		}

		return NoContent();
	}
	[HttpGet("{eventId:guid}/cards/{cardCode}/state"), AllowAnonymous]
	public async Task<ActionResult<object>> CardState(Guid eventId, string cardCode)
	{
		var card = await db.Cards.SingleOrDefaultAsync(x => x.EventId == eventId && x.PublicCode == cardCode); if (card is null) return NotFound(); var round = await db.Rounds.Include(x => x.Stages).Include(x => x.DrawnNumbers).Where(x => x.EventId == eventId && (x.Status == RoundStatus.Drawing || x.Status == RoundStatus.WinnerDetected || x.Status == RoundStatus.TieBreaker)).OrderByDescending(x => x.Sequence).FirstOrDefaultAsync();
		var marks = round is null ? [] : (await db.CardMarks.Where(x => x.RoundId == round.Id && x.CardId == card.Id).ToListAsync()).OrderBy(x => x.MarkedAt).Select(x => x.Number).ToArray(); var e = await db.Events.FindAsync(eventId);
		var previousRound = await db.Rounds.Where(item => item.EventId == eventId && item.Status == RoundStatus.Finished).OrderByDescending(item => item.Sequence).FirstOrDefaultAsync();
		var canGenerateNextCard = previousRound is not null
			&& !card.ReplacementCardId.HasValue
			&& !await db.Rounds.AnyAsync(item => item.EventId == eventId && item.Sequence > previousRound.Sequence && (item.Status == RoundStatus.Drawing || item.Status == RoundStatus.WinnerDetected || item.Status == RoundStatus.TieBreaker))
			&& await db.RoundEligibleCards.AnyAsync(item => item.RoundId == previousRound.Id && item.CardId == card.Id)
			&& card.IsComplete(await GetMarkedNumbers(eventId, card.Id, previousRound.Id));
		var activeStage = round?.Stages.SingleOrDefault(item => item.IsActive);
		return Ok(new { card.Id, card.PublicCode, numbers = ToRows(card.Numbers), markingMode = e!.MarkingMode, roundId = round?.Id, roundStatus = round?.Status, currentPrize = activeStage?.PrizeName, currentPattern = activeStage?.Pattern, drawnNumbers = round?.DrawnNumbers.OrderBy(x => x.Sequence).Select(x => x.Number) ?? [], markedNumbers = marks, lastSequence = round?.DrawnNumbers.Count ?? 0, canGenerateNextCard });
	}
	private async Task<int[]> GetMarkedNumbers(Guid eventId, Guid cardId, Guid roundId)
	{
		var bingoEvent = await db.Events.FindAsync(eventId) ?? throw new InvalidOperationException("Evento não encontrado.");
		return bingoEvent.MarkingMode == CardMarkingMode.Automatic
			? await db.DrawnNumbers.Where(item => item.RoundId == roundId).Select(item => item.Number).ToArrayAsync()
			: await db.CardMarks.Where(item => item.RoundId == roundId && item.CardId == cardId).Select(item => item.Number).ToArrayAsync();
	}
	private static int[][] ToRows(int[,] card) => Enumerable.Range(0, 5).Select(r => Enumerable.Range(0, 5).Select(c => card[r, c]).ToArray()).ToArray();
	private static bool IsValidPrizeImage(string? imageDataUrl) => string.IsNullOrWhiteSpace(imageDataUrl) || imageDataUrl.Length <= MaximumPrizeImageLength && SupportedPrizeImagePrefixes.Any(prefix => imageDataUrl.StartsWith(prefix, StringComparison.Ordinal));
	private static bool HasDuplicatePatterns(IEnumerable<CreatePrizeStageRequest> stages) => stages.GroupBy(stage => stage.Pattern).Any(group => group.Count() > 1);
	private Guid GetCompanyId() => Guid.Parse(User.FindFirstValue("company_id")!);
}
public sealed record CreateEventRequest(string Name, int CardsPerParticipant = 1, CardMarkingMode MarkingMode = CardMarkingMode.Automatic);
public sealed record GeneratePrintedCardsRequest(int Quantity);
public sealed record AssignPrintedCardRequest(Guid ParticipantId);
public sealed record CreateRoundRequest(string Name, IReadOnlyList<CreatePrizeStageRequest> Stages);
public sealed record CreatePrizeStageRequest(int Sequence, string PrizeName, WinningPattern Pattern, string? PrizeImageDataUrl = null);
public sealed record MarkNumberRequest(int Number);
