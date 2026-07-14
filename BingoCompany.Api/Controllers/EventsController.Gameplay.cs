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
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/draw")]
	public async Task<ActionResult<object>> Draw(Guid eventId, Guid roundId)
	{
		var e = await db.Events.FindAsync(eventId); var round = await db.Rounds.Include(x => x.Stages).Include(x => x.DrawnNumbers).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId); if (e is null || round is null) return NotFound();
		var eligibleCards = await db.Cards
			.AsNoTracking()
			.Join(db.RoundEligibleCards.Where(item => item.RoundId == roundId), card => card.Id, eligibleCard => eligibleCard.CardId, (card, _) => card)
			.ToListAsync();
		var marks = e.MarkingMode == CardMarkingMode.Automatic ? [] : await db.CardMarks.Where(item => item.RoundId == roundId).ToListAsync();
		var excludedCardIds = await db.RoundWinners.Where(item => item.RoundId == roundId && item.StageId == round.ActiveStage.Id).Select(item => item.CardId).ToHashSetAsync();
		RoundDrawResult drawResult;
		try
		{
			drawResult = gameplayService.Draw(round, eligibleCards, marks, e.MarkingMode, excludedCardIds);
		}
		catch (InvalidOperationException exception)
		{
			return Conflict($"{exception.Message} Cancele a rodada para encerrar o sorteio sem vencedor.");
		}
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
		if (candidates.Any(candidate => candidate.RevealedAt.HasValue)) return Conflict("O vencedor desta etapa já foi revelado.");
		var revealResult = gameplayService.RevealWinner(round, candidates, DateTimeOffset.UtcNow);
		var winner = revealResult.Winner;
		db.AuditEntries.Add(new AuditEntry(eventId, candidates.Count > 1 ? "Desempate concluído" : "Prêmio revelado", $"Prêmio {round.Stages.Single(stage => stage.Id == winner.StageId).PrizeName} revelado."));
		try
		{
			await db.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return Conflict("O vencedor desta etapa já foi revelado.");
		}
		var participantNames = await db.Participants
			.Where(participant => candidates.Select(candidate => candidate.ParticipantId).Contains(participant.Id))
			.ToDictionaryAsync(participant => participant.Id, participant => participant.Name);
		var card = await db.Cards.FindAsync(winner.CardId);
		var prize = round.Stages.Single(stage => stage.Id == winner.StageId).PrizeName;
		var result = new
		{
			participantName = participantNames[winner.ParticipantId],
			cardCode = card!.PublicCode,
			prize,
			tieBreakers = candidates.Select(candidate => new { participantName = participantNames[candidate.ParticipantId], candidate.TieBreakerNumber, candidate.IsWinner })
		};
		var stageChanged = new { roundId, currentPrize = round.Stages.SingleOrDefault(stage => stage.IsActive)?.PrizeName, status = round.Status };
		await hub.Clients.Group($"round:{roundId}").SendAsync("PrizeStageChanged", stageChanged);
		await hub.Clients.Group($"event:{eventId}").SendAsync("PrizeStageChanged", stageChanged);
		await hub.Clients.Group($"round:{roundId}").SendAsync("WinnerRevealed", result);
		await hub.Clients.Group($"event:{eventId}").SendAsync("WinnerRevealed", result); return Ok(result);
	}
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/prize-delivered")]
	public async Task<IActionResult> MarkPrizeDelivered(Guid eventId, Guid roundId)
	{
		var round = await db.Rounds.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId);
		if (round is null) return NotFound();
		var winner = await FindPendingWinner(roundId, round.ActiveStage.Id);
		if (winner is null) return Conflict("Não há prêmio aguardando confirmação de entrega.");

		winner.MarkPrizeDelivered(DateTimeOffset.UtcNow);
		round.FinishStage();
		db.AuditEntries.Add(new AuditEntry(eventId, "Prêmio entregue", $"O prêmio {round.Stages.Single(stage => stage.Id == winner.StageId).PrizeName} foi entregue ao vencedor."));
		if (round.Status == RoundStatus.Finished) db.AuditEntries.Add(new AuditEntry(eventId, "Rodada encerrada", $"Rodada {round.Name} encerrada."));
		await db.SaveChangesAsync();

		var stageChanged = new { roundId, currentPrize = round.Stages.SingleOrDefault(stage => stage.IsActive)?.PrizeName, status = round.Status };
		await hub.Clients.Group($"round:{roundId}").SendAsync("PrizeStageChanged", stageChanged);
		await hub.Clients.Group($"event:{eventId}").SendAsync("PrizeStageChanged", stageChanged);
		if (round.Status == RoundStatus.Finished)
		{
			await hub.Clients.Group($"round:{roundId}").SendAsync("RoundFinished", new { roundId });
			await hub.Clients.Group($"event:{eventId}").SendAsync("RoundFinished", new { roundId });
		}
		return NoContent();
	}
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/prize-declined")]
	public async Task<IActionResult> MarkPrizeDeclined(Guid eventId, Guid roundId)
	{
		var round = await db.Rounds.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId);
		if (round is null) return NotFound();
		var winner = await FindPendingWinner(roundId, round.ActiveStage.Id);
		if (winner is null) return Conflict("Não há prêmio aguardando confirmação de retirada.");

		winner.MarkPrizeDeclined(DateTimeOffset.UtcNow);
		round.ResumeDrawingAfterPrizeDeclined();
		db.AuditEntries.Add(new AuditEntry(eventId, "Prêmio não retirado", $"O vencedor do prêmio {round.ActiveStage.PrizeName} não retirou o prêmio; o sorteio continuará com a mesma regra."));
		await db.SaveChangesAsync();
		await hub.Clients.Group($"round:{roundId}").SendAsync("WinnerPresentationClosed", new { roundId });
		await hub.Clients.Group($"event:{eventId}").SendAsync("WinnerPresentationClosed", new { roundId });
		return NoContent();
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

}

