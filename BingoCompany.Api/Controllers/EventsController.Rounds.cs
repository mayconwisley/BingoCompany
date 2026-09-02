using BingoCompany.Api.Contracts;
using BingoCompany.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BingoCompany.Api.Controllers;

public sealed partial class EventsController
{
	[HttpPost("{eventId:guid}/rounds")]
	public async Task<ActionResult<object>> CreateRound(Guid eventId, CreateRoundRequest request, CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await eventRoundManagementService.Create(eventId, ToRoundConfiguration(request), cancellationToken);
			return result is null ? NotFound() : Ok(result);
		}
		catch (ArgumentException exception)
		{
			return BadRequest(exception.Message);
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

	[HttpPut("{eventId:guid}/rounds/{roundId:guid}")]
	public async Task<IActionResult> UpdateRound(Guid eventId, Guid roundId, CreateRoundRequest request, CancellationToken cancellationToken = default)
	{
		try
		{
			return await eventRoundManagementService.Update(eventId, roundId, ToRoundConfiguration(request), cancellationToken) ? NoContent() : NotFound();
		}
		catch (ArgumentException exception)
		{
			return BadRequest(exception.Message);
		}
		catch (RoundConcurrencyException exception)
		{
			return Conflict(exception.Message);
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/start")]
	public async Task<IActionResult> StartRound(Guid eventId, Guid roundId, CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await eventRoundManagementService.Start(eventId, roundId, cancellationToken);
			if (result is null) return NotFound();
			await hub.Clients.Group($"round:{roundId}").SendAsync("RoundStarted", new { roundId, result.SequenceHash }, cancellationToken);
			return NoContent();
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

	[HttpPost("{eventId:guid}/rounds/{roundId:guid}/cancel")]
	public async Task<IActionResult> CancelRound(Guid eventId, Guid roundId, CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await eventRoundManagementService.Cancel(eventId, roundId, cancellationToken);
			if (result is null) return NotFound();
			var notice = new { roundId, cancelled = true };
			await hub.Clients.Group($"round:{roundId}").SendAsync("RoundFinished", notice, cancellationToken);
			await hub.Clients.Group($"event:{eventId}").SendAsync("RoundFinished", notice, cancellationToken);
			return NoContent();
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

	[HttpPost("{eventId:guid}/finish")]
	public async Task<IActionResult> FinishEvent(Guid eventId, CancellationToken cancellationToken = default)
	{
		try
		{
			if (!await eventRoundManagementService.FinishEvent(eventId, cancellationToken)) return NotFound();
			await hub.Clients.Group($"event:{eventId}").SendAsync("EventFinished", new { eventId }, cancellationToken);
			return NoContent();
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

	private static RoundConfigurationRequest ToRoundConfiguration(CreateRoundRequest request) => new(request.Name, request.Stages.Select(stage => new RoundStageDefinition(stage.PrizeName, stage.Pattern, stage.PrizeImageDataUrl)).ToArray());
}
