using BingoCompany.Api.Hubs;
using BingoCompany.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BingoCompany.Api.Controllers;

public sealed partial class EventsController
{
	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/draw")]
	public async Task<ActionResult<object>> Draw(Guid eventId, Guid roundId, CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await Gameplay.Draw(eventId, roundId, cancellationToken);
			if (result is null) return NotFound();
			var message = new { roundId, number = result.Number, sequence = result.Sequence, winnersDetected = result.WinnersDetected };
			await Broadcast("NumberDrawn", roundId, eventId, message, cancellationToken);
			if (result.WinnersDetected > 0)
			{
				var notice = new { roundId, count = result.WinnersDetected, tieBreakerRequired = result.TieBreakerRequired };
				await Broadcast("WinnerDetected", roundId, eventId, notice, cancellationToken);
				await Broadcast("WinningCardDetected", roundId, eventId, notice, cancellationToken);
				if (result.TieBreakerRequired) await Broadcast("TieBreakerStarted", roundId, eventId, notice, cancellationToken);
			}
			return Ok(message);
		}
		catch (InvalidOperationException exception) { return Conflict($"{exception.Message} Cancele a rodada para encerrar o sorteio sem vencedor."); }
	}

	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/reveal")]
	public async Task<ActionResult<object>> Reveal(Guid eventId, Guid roundId, CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await Gameplay.Reveal(eventId, roundId, cancellationToken);
			var response = new { participantName = result.ParticipantName, cardCode = result.CardCode, prize = result.Prize, tieBreakers = result.TieBreakers.Select(item => new { participantName = item.ParticipantName, item.TieBreakerNumber, item.IsWinner }) };
			var stageChanged = new { roundId, currentPrize = result.CurrentPrize, status = result.Status };
			await Broadcast("PrizeStageChanged", roundId, eventId, stageChanged, cancellationToken);
			await Broadcast("WinnerRevealed", roundId, eventId, response, cancellationToken);
			return Ok(response);
		}
		catch (KeyNotFoundException) { return NotFound(); }
		catch (InvalidOperationException exception) { return Conflict(exception.Message); }
	}

	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/prize-delivered")]
	public Task<IActionResult> MarkPrizeDelivered(Guid eventId, Guid roundId, CancellationToken cancellationToken = default) => ProcessStageOperation(eventId, roundId, Gameplay.MarkDelivered, cancellationToken);

	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/prize-declined")]
	public async Task<IActionResult> MarkPrizeDeclined(Guid eventId, Guid roundId, CancellationToken cancellationToken = default)
	{
		var response = await ProcessStageOperation(eventId, roundId, Gameplay.MarkDeclined, cancellationToken);
		if (response is NoContentResult) await Broadcast("WinnerPresentationClosed", roundId, eventId, new { roundId }, cancellationToken);
		return response;
	}

	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/winner-presentation/close")]
	public async Task<IActionResult> CloseWinnerPresentation(Guid eventId, Guid roundId, CancellationToken cancellationToken = default)
	{
		var response = await ProcessStageOperation(eventId, roundId, Gameplay.ClosePresentation, cancellationToken);
		if (response is NoContentResult) await Broadcast("WinnerPresentationClosed", roundId, eventId, new { roundId }, cancellationToken);
		return response;
	}

	private IRoundGameplayCoordinator Gameplay => roundGameplayCoordinator ?? throw new InvalidOperationException("Serviço de gameplay não configurado.");
	private async Task<IActionResult> ProcessStageOperation(Guid eventId, Guid roundId, Func<Guid, Guid, CancellationToken, Task<BingoCompany.Application.Services.PrizeStageOperationResult?>> operation, CancellationToken cancellationToken)
	{
		try
		{
			var result = await operation(eventId, roundId, cancellationToken);
			if (result is null) return NotFound();
			await Broadcast("PrizeStageChanged", roundId, eventId, new { roundId, currentPrize = result.CurrentPrize, status = result.Status }, cancellationToken);
			if (result.Status == BingoCompany.Domain.Enums.RoundStatus.Finished) await Broadcast("RoundFinished", roundId, eventId, new { roundId }, cancellationToken);
			return NoContent();
		}
		catch (InvalidOperationException exception) { return Conflict(exception.Message); }
	}
	private async Task Broadcast(string eventName, Guid roundId, Guid eventId, object message, CancellationToken cancellationToken)
	{
		await hub.Clients.Group($"round:{roundId}").SendAsync(eventName, message, cancellationToken);
		await hub.Clients.Group($"event:{eventId}").SendAsync(eventName, message, cancellationToken);
	}
}
