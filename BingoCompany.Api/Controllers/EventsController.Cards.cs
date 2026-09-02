using BingoCompany.Api.Contracts;
using BingoCompany.Api.Hubs;
using BingoCompany.Api.Security;
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
	public async Task<ActionResult<object>> GenerateNextCard(Guid eventId, string cardCode, CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await cardLifecycleService.GenerateNext(eventId, cardCode, cancellationToken);
			return result is null ? NotFound() : Ok(new { cardId = result.CardId, result.PublicCode, numbers = result.Numbers });
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}
	[HttpPost("{eventId:guid}/cards/printed")]
	public async Task<ActionResult<object>> GeneratePrintedCards(Guid eventId, GeneratePrintedCardsRequest request)
	{
		try
		{
			var cards = await cardLifecycleService.GeneratePrinted(eventId, request.Quantity, HttpContext.RequestAborted);
			return cards is null ? NotFound() : Ok(cards.Select(card => new { card.PublicCode, card.Fingerprint, qrCodeValue = $"/cartelas/{card.PublicCode}" }));
		}
		catch (ArgumentOutOfRangeException exception)
		{
			return BadRequest(exception.Message);
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}
	[HttpGet("{eventId:guid}/cards/printed")]
	public async Task<ActionResult<object>> GetPrintedCards(Guid eventId)
	{
		var cards = await cardLifecycleService.GetPrinted(eventId, HttpContext.RequestAborted);
		return cards is null ? NotFound() : Ok(cards);
	}
	[HttpPost("{eventId:guid}/cards/{cardCode}/assign")]
	public async Task<IActionResult> AssignPrintedCard(Guid eventId, string cardCode, AssignPrintedCardRequest request)
	{
		try
		{
			return await cardLifecycleService.AssignPrinted(eventId, cardCode, request.ParticipantId, HttpContext.RequestAborted) ? NoContent() : NotFound();
		}
		catch (InvalidOperationException exception)
		{
			return BadRequest(exception.Message);
		}
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
		try
		{
			return await cardLifecycleService.ActivatePrinted(eventId, cardCode, HttpContext.RequestAborted) ? NoContent() : NotFound();
		}
		catch (InvalidOperationException exception)
		{
			return BadRequest(exception.Message);
		}
	}

	[HttpPost("{eventId:guid}/cards/{cardCode}/marks")]
	public async Task<IActionResult> Mark(Guid eventId, string cardCode, MarkNumberRequest request, CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await (manualCardMarkingService ?? throw new InvalidOperationException("Serviço de marcação manual não configurado.")).Mark(eventId, cardCode, request.Number, cancellationToken);
			if (!result.Exists) return NotFound();
			if (result.WinnerDetected)
			{
				var notice = new { roundId = result.RoundId, count = 1 };
				await hub.Clients.Group($"round:{result.RoundId}").SendAsync("WinningCardDetected", notice, cancellationToken);
				await hub.Clients.Group($"event:{eventId}").SendAsync("WinningCardDetected", notice, cancellationToken);
			}
			return NoContent();
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
	public async Task<ActionResult<object>> CardState(Guid eventId, string cardCode, CancellationToken cancellationToken = default)
	{
		var state = await cardStateQueryService.Get(eventId, cardCode, cancellationToken);
		return state is null
			? NotFound()
			: Ok(new
			{
				state.Id,
				state.PublicCode,
				state.ParticipantName,
				state.ResponsibleEmployeeName,
				state.IsWinner,
				state.Numbers,
				eventStatus = state.EventStatus.ToString(),
				markingMode = state.MarkingMode,
				roundId = state.RoundId,
				roundStatus = state.RoundStatus,
				currentPrize = state.CurrentPrize,
				currentPattern = state.CurrentPattern,
				remainingNumbersToWin = state.RemainingNumbersToWin,
				drawnNumbers = state.DrawnNumbers,
				markedNumbers = state.MarkedNumbers,
				lastSequence = state.LastSequence,
				canGenerateNextCard = state.CanGenerateNextCard
			});
	}

}

