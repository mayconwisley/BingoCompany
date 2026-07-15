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
		round.Update(request.Name, stages);
		db.PrizeStages.AddRange(stages);
		db.AuditEntries.Add(new AuditEntry(eventId, "Rodada editada", $"Rodada {round.Name} atualizada com {stages.Length} etapas."));
		try
		{
			await db.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return Conflict("A rodada foi alterada por outra operação. Recarregue a página antes de tentar novamente.");
		}
		return NoContent();
	}
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/start")]
	public async Task<IActionResult> StartRound(Guid eventId, Guid roundId)
	{
		var e = await db.Events.FindAsync(eventId); var round = await db.Rounds.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == roundId && x.EventId == eventId); if (e is null || round is null) return NotFound();
		if (e.Status == EventStatus.Finished) return Conflict("O evento encerrado não pode ser alterado.");
		if (!await db.Cards.AnyAsync(item => item.EventId == eventId && item.Status == CardStatus.Active && item.ParticipantId.HasValue)) return Conflict("Gere e ative ao menos uma cartela associada a participante antes de abrir a operação.");
		if (e.Status == EventStatus.RegistrationOpen) e.Start();
		round.FreezeEligibility(await db.Cards.Where(x => x.EventId == eventId).ToListAsync());
		db.RoundEligibleCards.AddRange(round.EligibleCards);
		var sequence = SecureDrawSequence.Generate();
		round.Start(sequence, SecureDrawSequence.Hash(sequence));
		db.AuditEntries.Add(new AuditEntry(eventId, "Rodada iniciada", $"Rodada {round.Name} iniciada. Hash {round.SequenceHash}."));
		await db.SaveChangesAsync();
		await hub.Clients.Group($"round:{roundId}").SendAsync("RoundStarted", new { roundId, round.SequenceHash }); return NoContent();
	}
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/cancel")]
	public async Task<IActionResult> CancelRound(Guid eventId, Guid roundId)
	{
		var bingoEvent = await db.Events.FindAsync(eventId);
		var round = await db.Rounds.Include(item => item.DrawnNumbers).SingleOrDefaultAsync(item => item.Id == roundId && item.EventId == eventId);
		if (bingoEvent is null || round is null) return NotFound();
		if (bingoEvent.Status == EventStatus.Finished) return Conflict("O evento encerrado não pode ser alterado.");
		try
		{
			round.Cancel();
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}

		db.AuditEntries.Add(new AuditEntry(eventId, "Rodada cancelada", $"Rodada {round.Name} cancelada após {round.DrawnNumbers.Count} pedras sorteadas."));
		await db.SaveChangesAsync();
		var notice = new { roundId, cancelled = true };
		await hub.Clients.Group($"round:{roundId}").SendAsync("RoundFinished", notice);
		await hub.Clients.Group($"event:{eventId}").SendAsync("RoundFinished", notice);
		return NoContent();
	}
	[HttpPost("{eventId:guid}/finish")]
	public async Task<IActionResult> FinishEvent(Guid eventId)
	{
		var bingoEvent = await db.Events.Include(item => item.Rounds).SingleOrDefaultAsync(item => item.Id == eventId);
		if (bingoEvent is null) return NotFound();
		bingoEvent.Finish();
		var unusedCards = await db.Cards.Where(item => item.EventId == eventId && item.Type == CardType.Digital && item.Status == CardStatus.Assigned).ToListAsync();
		foreach (var card in unusedCards) card.InvalidateUnused();
		db.AuditEntries.Add(new AuditEntry(eventId, "Evento encerrado", "O evento foi encerrado e tornou-se imutável."));
		if (unusedCards.Count > 0) db.AuditEntries.Add(new AuditEntry(eventId, "Cartelas não utilizadas invalidadas", $"{unusedCards.Count} cartela(s) digital(is) não ativada(s) foram invalidadas."));
		await db.SaveChangesAsync();
		await hub.Clients.Group($"event:{eventId}").SendAsync("EventFinished", new { eventId });
		return NoContent();
	}

}

