using BingoCompany.Application;
using BingoCompany.Domain;
using BingoCompany.Infrastructure;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BingoCompany.Api.Hubs;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/events")]
public sealed class EventsController(BingoDbContext db, IHubContext<BingoHub> hub) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> List() => Ok((await db.Events.Select(x => new { x.Id, x.Name, x.PublicCode, x.Status, x.MarkingMode, x.CreatedAt }).ToListAsync()).OrderByDescending(x => x.CreatedAt));
    [HttpPost]
    public async Task<ActionResult<object>> Create(CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Informe o nome do evento.");
        var bingoEvent = new BingoEvent(request.Name, request.CardsPerParticipant <= 0 ? 1 : request.CardsPerParticipant, request.MarkingMode);
        db.Events.Add(bingoEvent); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { eventId = bingoEvent.Id }, new { bingoEvent.Id, bingoEvent.Name, bingoEvent.PublicCode, bingoEvent.Status });
    }
    [HttpGet("{eventId:guid}")]
    public async Task<ActionResult<object>> Get(Guid eventId)
    {
        var e = await db.Events.Include(x => x.Rounds).Include(x => x.Cards).Include(x => x.Participants).SingleOrDefaultAsync(x => x.Id == eventId);
        return e is null ? NotFound() : Ok(new { e.Id, e.Name, e.PublicCode, e.Status, participants = e.Participants.Count, cards = e.Cards.Count, rounds = e.Rounds.OrderBy(x => x.Sequence).Select(x => new { x.Id, x.Name, x.Status }) });
    }
    [HttpPost("{eventId:guid}/registration/open")]
    public async Task<IActionResult> OpenRegistration(Guid eventId) { var e = await db.Events.FindAsync(eventId); if (e is null) return NotFound(); e.OpenRegistration(); await db.SaveChangesAsync(); return NoContent(); }
    [HttpPost("{eventId:guid}/participants")]
    public async Task<ActionResult<object>> Join(Guid eventId, JoinEventRequest request)
    {
        var e = await db.Events.Include(x => x.Cards).SingleOrDefaultAsync(x => x.Id == eventId); if (e is null) return NotFound();
        if (e.Status != EventStatus.RegistrationOpen) return Conflict("As inscrições estão fechadas.");
        var participant = new Participant(eventId, request.Name, request.ResponsibleEmployeeName);
        var card = new BingoCard(eventId, participant.Id, CardType.Digital, new Bingo75CardGenerator().Generate());
        db.Participants.Add(participant);
        db.Cards.Add(card);
        await db.SaveChangesAsync();
        return Ok(new { participantId = participant.Id, cardId = card.Id, card.PublicCode, numbers = ToRows(card.Numbers) });
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
        if (await db.Rounds.AnyAsync(item => item.EventId == eventId && item.Sequence > previousRound.Sequence && (item.Status == RoundStatus.Drawing || item.Status == RoundStatus.WinnerDetected))) return Conflict("A próxima rodada já foi iniciada.");
        if (!await db.RoundEligibleCards.AnyAsync(item => item.RoundId == previousRound.Id && item.CardId == card.Id)) return Conflict("Esta cartela não participou da última rodada concluída.");

        var markedNumbers = await GetMarkedNumbers(eventId, card.Id, previousRound.Id);
        if (!card.IsComplete(markedNumbers)) return Conflict("A cartela anterior ainda não está completa.");

        var nextCard = new BingoCard(eventId, card.ParticipantId, CardType.Digital, new Bingo75CardGenerator().Generate());
        card.ReplaceWith(nextCard);
        db.Cards.Add(nextCard);
        await db.SaveChangesAsync();
        return Ok(new { cardId = nextCard.Id, nextCard.PublicCode, numbers = ToRows(nextCard.Numbers) });
    }
    [HttpPost("{eventId:guid}/rounds")]
    public async Task<ActionResult<object>> CreateRound(Guid eventId, CreateRoundRequest request)
    {
        if (!await db.Events.AnyAsync(x => x.Id == eventId)) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Informe o nome da rodada.");
        if (request.Stages.Count == 0) return BadRequest("Configure ao menos uma etapa de prêmio.");
        if (request.Stages.Any(stage => string.IsNullOrWhiteSpace(stage.PrizeName))) return BadRequest("Informe o nome de cada prêmio.");
        if (request.Stages.Select(stage => stage.Sequence).Distinct().Count() != request.Stages.Count) return BadRequest("A sequência das etapas de prêmio não pode se repetir.");
        var count = await db.Rounds.CountAsync(x => x.EventId == eventId); var round = new BingoRound(eventId, count + 1, request.Name);
        foreach (var stage in request.Stages.OrderBy(x => x.Sequence)) round.AddStage(new PrizeStage(round.Id, stage.Sequence, stage.PrizeName, stage.Pattern));
        db.Rounds.Add(round); await db.SaveChangesAsync(); return Ok(new { round.Id, round.Name });
    }
    [HttpPost("{eventId:guid}/rounds/{roundId:guid}/start")]
    public async Task<IActionResult> StartRound(Guid eventId, Guid roundId)
    {
        var e = await db.Events.FindAsync(eventId); var round = await db.Rounds.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId); if (e is null || round is null) return NotFound();
        if (e.Status == EventStatus.RegistrationOpen) e.Start();
        round.FreezeEligibility(await db.Cards.Where(x => x.EventId == eventId).ToListAsync());
        db.RoundEligibleCards.AddRange(round.EligibleCards);
        var sequence = SecureDrawSequence.Generate();
        round.Start(sequence, SecureDrawSequence.Hash(sequence));
        await db.SaveChangesAsync();
        await hub.Clients.Group($"round:{roundId}").SendAsync("RoundStarted", new { roundId, round.SequenceHash }); return NoContent();
    }
    [HttpPost("{eventId:guid}/rounds/{roundId:guid}/draw")]
    public async Task<ActionResult<object>> Draw(Guid eventId, Guid roundId)
    {
        var e = await db.Events.FindAsync(eventId); var round = await db.Rounds.Include(x => x.Stages).Include(x => x.DrawnNumbers).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId); if (e is null || round is null) return NotFound();
        var drawn = round.DrawNext();
        db.DrawnNumbers.Add(drawn);
        var eligible = await db.RoundEligibleCards.Where(x => x.RoundId == roundId).Select(x => x.CardId).ToListAsync(); var cards = await db.Cards.Where(x => eligible.Contains(x.Id)).ToListAsync(); var numbers = round.DrawnNumbers.Select(x => x.Number).ToHashSet();
        var marks = e.MarkingMode == CardMarkingMode.Automatic ? null : await db.CardMarks.Where(x => x.RoundId == roundId).ToListAsync();
        var winners = cards.Where(card => WinningPatternEvaluator.IsCompleted(card.Numbers, (marks?.Where(x => x.CardId == card.Id).Select(x => x.Number).ToHashSet() ?? numbers), round.ActiveStage.Pattern)).ToArray();
        if (winners.Length > 0) { round.DetectWinner(); foreach (var card in winners) db.RoundWinners.Add(new RoundWinner(round.Id, round.ActiveStage.Id, card.Id, card.ParticipantId!.Value, drawn.Sequence)); } await db.SaveChangesAsync();
        var message = new { roundId, number = drawn.Number, sequence = drawn.Sequence, winnersDetected = winners.Length }; await hub.Clients.Group($"round:{roundId}").SendAsync("NumberDrawn", message); await hub.Clients.Group($"event:{eventId}").SendAsync("NumberDrawn", message); if (winners.Length > 0) { var notice = new { roundId, count = winners.Length }; await hub.Clients.Group($"round:{roundId}").SendAsync("WinningCardDetected", notice); await hub.Clients.Group($"event:{eventId}").SendAsync("WinningCardDetected", notice); } return Ok(message);
    }
    [HttpPost("{eventId:guid}/rounds/{roundId:guid}/reveal")]
    public async Task<ActionResult<object>> Reveal(Guid eventId, Guid roundId)
    {
        var round = await db.Rounds.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId); if (round is null || round.Status != RoundStatus.WinnerDetected) return Conflict("Não há vencedor aguardando revelação.");
        var candidates = await db.RoundWinners.Where(x => x.RoundId == roundId && x.StageId == round.ActiveStage.Id).ToListAsync(); if (candidates.Count == 0) return NotFound();
        if (candidates.Count > 1) { var tie = SecureDrawSequence.Generate().Take(candidates.Count).ToArray(); for (var i = 0; i < candidates.Count; i++) candidates[i].AssignTieBreaker(tie[i]); }
        var winner = candidates.OrderByDescending(x => x.TieBreakerNumber ?? 0).First(); winner.Confirm(DateTimeOffset.UtcNow); round.FinishStage(); await db.SaveChangesAsync();
        var participant = await db.Participants.FindAsync(winner.ParticipantId); var card = await db.Cards.FindAsync(winner.CardId); var result = new { participantName = participant!.Name, cardCode = card!.PublicCode, prize = candidates.Count > 0 ? (await db.PrizeStages.FindAsync(winner.StageId))!.PrizeName : "", tieBreakers = candidates.Select(x => new { participantName = db.Participants.Find(x.ParticipantId)!.Name, x.TieBreakerNumber, x.IsWinner }) };
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
        var hasWon = WinningPatternEvaluator.IsCompleted(card.Numbers, markedNumbers, round.ActiveStage.Pattern);
        if (hasWon)
        {
            round.DetectWinner();
            db.RoundWinners.Add(new RoundWinner(round.Id, round.ActiveStage.Id, card.Id, card.ParticipantId!.Value, drawn.Sequence));
        }

        await db.SaveChangesAsync();
        if (hasWon)
        {
            var notice = new { roundId = round.Id, count = 1 };
            await hub.Clients.Group($"round:{round.Id}").SendAsync("WinningCardDetected", notice);
            await hub.Clients.Group($"event:{eventId}").SendAsync("WinningCardDetected", notice);
        }

        return NoContent();
    }
    [HttpGet("{eventId:guid}/cards/{cardCode}/state")]
    public async Task<ActionResult<object>> CardState(Guid eventId, string cardCode)
    {
        var card = await db.Cards.SingleOrDefaultAsync(x => x.EventId == eventId && x.PublicCode == cardCode); if (card is null) return NotFound(); var round = await db.Rounds.Include(x => x.Stages).Include(x => x.DrawnNumbers).Where(x => x.EventId == eventId && (x.Status == RoundStatus.Drawing || x.Status == RoundStatus.WinnerDetected)).OrderByDescending(x => x.Sequence).FirstOrDefaultAsync();
        var marks = round is null ? [] : (await db.CardMarks.Where(x => x.RoundId == round.Id && x.CardId == card.Id).ToListAsync()).OrderBy(x => x.MarkedAt).Select(x => x.Number).ToArray(); var e = await db.Events.FindAsync(eventId);
        var previousRound = await db.Rounds.Where(item => item.EventId == eventId && item.Status == RoundStatus.Finished).OrderByDescending(item => item.Sequence).FirstOrDefaultAsync();
        var canGenerateNextCard = previousRound is not null
            && !card.ReplacementCardId.HasValue
            && !await db.Rounds.AnyAsync(item => item.EventId == eventId && item.Sequence > previousRound.Sequence && (item.Status == RoundStatus.Drawing || item.Status == RoundStatus.WinnerDetected))
            && await db.RoundEligibleCards.AnyAsync(item => item.RoundId == previousRound.Id && item.CardId == card.Id)
            && card.IsComplete(await GetMarkedNumbers(eventId, card.Id, previousRound.Id));
        return Ok(new { card.Id, card.PublicCode, numbers = ToRows(card.Numbers), markingMode = e!.MarkingMode, roundId = round?.Id, roundStatus = round?.Status, currentPrize = round?.Stages.SingleOrDefault(x => x.IsActive)?.PrizeName, drawnNumbers = round?.DrawnNumbers.OrderBy(x => x.Sequence).Select(x => x.Number) ?? [], markedNumbers = marks, lastSequence = round?.DrawnNumbers.Count ?? 0, canGenerateNextCard });
    }
    private async Task<int[]> GetMarkedNumbers(Guid eventId, Guid cardId, Guid roundId)
    {
        var bingoEvent = await db.Events.FindAsync(eventId) ?? throw new InvalidOperationException("Evento não encontrado.");
        return bingoEvent.MarkingMode == CardMarkingMode.Automatic
            ? await db.DrawnNumbers.Where(item => item.RoundId == roundId).Select(item => item.Number).ToArrayAsync()
            : await db.CardMarks.Where(item => item.RoundId == roundId && item.CardId == cardId).Select(item => item.Number).ToArrayAsync();
    }
    private static int[][] ToRows(int[,] card) => Enumerable.Range(0, 5).Select(r => Enumerable.Range(0, 5).Select(c => card[r, c]).ToArray()).ToArray();
}
public sealed record CreateEventRequest(string Name, int CardsPerParticipant = 1, CardMarkingMode MarkingMode = CardMarkingMode.Automatic);
public sealed record JoinEventRequest(string Name, string? ResponsibleEmployeeName = null);
public sealed record CreateRoundRequest(string Name, IReadOnlyList<CreatePrizeStageRequest> Stages);
public sealed record CreatePrizeStageRequest(int Sequence, string PrizeName, WinningPattern Pattern);
public sealed record MarkNumberRequest(int Number);
