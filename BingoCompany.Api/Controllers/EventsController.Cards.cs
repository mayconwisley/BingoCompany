using BingoCompany.Api.Contracts;
using BingoCompany.Api.Hubs;
using BingoCompany.Api.Interfaces;
using BingoCompany.Api.Security;
using BingoCompany.Api.Services;
using BingoCompany.Application;
using BingoCompany.Application.Interfaces;
using BingoCompany.Application.Services;
using BingoCompany.Domain;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Api.Controllers;

public sealed partial class EventsController
{
	[HttpPost("{eventId:guid}/cards/{cardCode}/next")]
	public async Task<ActionResult<object>> GenerateNextCard(Guid eventId, string cardCode)
	{
		var bingoEvent = await db.Events.FindAsync(eventId);
		if (bingoEvent is null) return NotFound();
		if (bingoEvent.Status == EventStatus.Finished) return Conflict("O evento encerrado não pode ser alterado.");
		var card = await db.Cards.SingleOrDefaultAsync(item => item.EventId == eventId && item.PublicCode == cardCode);
		if (card is null) return NotFound();
		if (card.ReplacementCardId.HasValue) return Conflict("Uma nova cartela já foi gerada a partir desta cartela.");

		var previousRound = await db.Rounds
			.Where(item => item.EventId == eventId && (item.Status == RoundStatus.Finished || item.Status == RoundStatus.Cancelled))
			.OrderByDescending(item => item.Sequence)
			.FirstOrDefaultAsync();
		if (previousRound is null) return Conflict("A cartela poderá ser renovada após o encerramento de uma rodada.");
		if (await db.Rounds.AnyAsync(item => item.EventId == eventId && item.Sequence > previousRound.Sequence && (item.Status == RoundStatus.Drawing || item.Status == RoundStatus.WinnerDetected || item.Status == RoundStatus.TieBreaker))) return Conflict("A próxima rodada já foi iniciada.");
		if (!await db.RoundEligibleCards.AnyAsync(item => item.RoundId == previousRound.Id && item.CardId == card.Id)) return Conflict("Esta cartela não participou da última rodada concluída.");

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
	[HttpPost("{eventId:guid}/cards/{cardCode}/register")]
	public async Task<ActionResult<object>> RegisterPrintedCard(Guid eventId, string cardCode, RegisterPrintedCardParticipantRequest request)
	{
		try
		{
			var result = await printedCardRegistrationService.Register(eventId, cardCode, new PrintedCardParticipantRegistration(request.Name, request.Type, request.EmployeeRegistration, request.ResponsibleEmployeeName), HttpContext.RequestAborted);
			return result is null ? NotFound() : Ok(result);
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
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

	[HttpPost("{eventId:guid}/cards/{cardCode}/marks")]
	public async Task<IActionResult> Mark(Guid eventId, string cardCode, MarkNumberRequest request)
	{
		var e = await db.Events.FindAsync(eventId); var card = await db.Cards.SingleOrDefaultAsync(x => x.EventId == eventId && x.PublicCode == cardCode);
		var round = await db.Rounds.Include(x => x.Stages).Include(x => x.DrawnNumbers).Where(x => x.EventId == eventId && x.Status == RoundStatus.Drawing).OrderByDescending(x => x.Sequence).FirstOrDefaultAsync();
		if (e is null || card is null || round is null) return NotFound();
		if (card.Type == CardType.Printed) return Conflict("Cartelas impressas devem ser conferidas pelo QR Code do operador.");
		if (e.MarkingMode == CardMarkingMode.Automatic) return Conflict("Este evento usa marcação automática.");
		if (!card.IsEligible || !Enumerable.Range(0, 5).SelectMany(r => Enumerable.Range(0, 5).Select(c => card.Numbers[r, c])).Contains(request.Number)) return BadRequest("Número inválido para a cartela.");
		var drawn = round.DrawnNumbers.SingleOrDefault(x => x.Number == request.Number); if (drawn is null) return BadRequest("O número ainda não foi sorteado.");
		if (e.MarkingMode == CardMarkingMode.ManualRequired && drawn.Sequence != round.DrawnNumbers.Max(item => item.Sequence)) return Conflict("Na marcação manual obrigatória, somente a pedra atual pode ser marcada.");
		if (await db.CardMarks.AnyAsync(x => x.RoundId == round.Id && x.CardId == card.Id && x.Number == request.Number)) return NoContent();
		db.CardMarks.Add(new CardMark(round.Id, card.Id, request.Number, drawn.Sequence));
		var markedNumbers = (await db.CardMarks.Where(x => x.RoundId == round.Id && x.CardId == card.Id).Select(x => x.Number).ToListAsync()).Append(request.Number).ToHashSet();
		var isCardExcluded = await db.RoundWinners.AnyAsync(item => item.RoundId == round.Id && item.StageId == round.ActiveStage.Id && item.CardId == card.Id);
		var winnerDetection = gameplayService.DetectManualWinner(round, card, markedNumbers, drawn.Sequence, isCardExcluded);
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
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/printed-cards/{cardCode}/validate-winner")]
	public async Task<ActionResult<object>> ValidatePrintedWinner(Guid eventId, Guid roundId, string cardCode)
	{
		try
		{
			var result = await printedWinnerValidationService.Validate(eventId, roundId, cardCode, HttpContext.RequestAborted);
			if (result is null) return NotFound();
			var notice = new { roundId, count = result.CandidatesCount, tieBreakerRequired = result.TieBreakerRequired };
			await hub.Clients.Group($"round:{roundId}").SendAsync("WinningCardDetected", notice);
			await hub.Clients.Group($"event:{eventId}").SendAsync("WinningCardDetected", notice);
			return Ok(new { result.ParticipantName, result.CardCode, result.TieBreakerRequired });
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}
	[HttpGet("{eventId:guid}/cards/{cardCode}/state"), AllowAnonymous]
	public async Task<ActionResult<object>> CardState(Guid eventId, string cardCode)
	{
		var card = await db.Cards.SingleOrDefaultAsync(x => x.EventId == eventId && x.PublicCode == cardCode); if (card is null) return NotFound(); var round = await db.Rounds.Include(x => x.Stages).Include(x => x.DrawnNumbers).Where(x => x.EventId == eventId && (x.Status == RoundStatus.Drawing || x.Status == RoundStatus.WinnerDetected || x.Status == RoundStatus.TieBreaker)).OrderByDescending(x => x.Sequence).FirstOrDefaultAsync();
		var participant = card.Type == CardType.Digital && card.ParticipantId.HasValue
			? await db.Participants.SingleOrDefaultAsync(item => item.Id == card.ParticipantId.Value && item.EventId == eventId)
			: null;
		var marks = round is null ? [] : (await db.CardMarks.Where(x => x.RoundId == round.Id && x.CardId == card.Id).ToListAsync()).OrderBy(x => x.MarkedAt).Select(x => x.Number).ToArray(); var e = await db.Events.FindAsync(eventId);
		var previousRound = await db.Rounds.Where(item => item.EventId == eventId && (item.Status == RoundStatus.Finished || item.Status == RoundStatus.Cancelled)).OrderByDescending(item => item.Sequence).FirstOrDefaultAsync();
		var canGenerateNextCard = previousRound is not null
			&& e!.Status != EventStatus.Finished
			&& !card.ReplacementCardId.HasValue
			&& !await db.Rounds.AnyAsync(item => item.EventId == eventId && item.Sequence > previousRound.Sequence && (item.Status == RoundStatus.Drawing || item.Status == RoundStatus.WinnerDetected || item.Status == RoundStatus.TieBreaker))
			&& await db.RoundEligibleCards.AnyAsync(item => item.RoundId == previousRound.Id && item.CardId == card.Id);
		var currentRoundSequence = round?.Sequence;
		var isWinner = await db.RoundWinners
			.Join(db.PrizeStages, winner => winner.StageId, stage => stage.Id, (winner, stage) => new { winner, stage })
			.Join(db.Rounds, item => item.winner.RoundId, winnerRound => winnerRound.Id, (item, winnerRound) => new { item.winner, item.stage, winnerRound })
			.AnyAsync(item =>
				item.winner.CardId == card.Id
				&& item.winner.IsWinner
				&& item.winner.RevealedAt.HasValue
				&& !item.winner.PrizeDeclinedAt.HasValue
				&& !item.stage.IsWinnerPresentationClosed
				&& (!currentRoundSequence.HasValue || item.winnerRound.Sequence >= currentRoundSequence.Value));
		var activeStage = round?.Stages.SingleOrDefault(item => item.IsActive);
		return Ok(new { card.Id, card.PublicCode, participantName = participant?.Name, responsibleEmployeeName = participant?.ResponsibleEmployeeName, isWinner, numbers = ToRows(card.Numbers), eventStatus = e!.Status.ToString(), markingMode = e.MarkingMode, roundId = round?.Id, roundStatus = round?.Status, currentPrize = activeStage?.PrizeName, currentPattern = activeStage?.Pattern, drawnNumbers = round?.DrawnNumbers.OrderBy(x => x.Sequence).Select(x => x.Number) ?? [], markedNumbers = marks, lastSequence = round?.DrawnNumbers.Count ?? 0, canGenerateNextCard });
	}

}

