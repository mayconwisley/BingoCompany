using BingoCompany.Api.Contracts;
using BingoCompany.Api.Hubs;
using BingoCompany.Application.Interfaces;
using BingoCompany.Application.Services;
using BingoCompany.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/events"), Authorize(Policy = "company-user"), ServiceFilter<CompanyEventOwnerFilter>]
public sealed partial class EventsController(IHubContext<BingoHub> hub, IEventParticipantRegistrationService participantRegistrationService, IEventCardPurchaseService cardPurchaseService, IPrintedWinnerValidationService printedWinnerValidationService, IPrintedCardRegistrationService printedCardRegistrationService, ICardStateQueryService cardStateQueryService, ICompanyEventsQueryService companyEventsQueryService, IEventConfigurationService eventConfigurationService, ICardLifecycleService cardLifecycleService, IEventRoundManagementService eventRoundManagementService, IManualCardMarkingService? manualCardMarkingService = null, IRoundGameplayCoordinator? roundGameplayCoordinator = null) : ControllerBase
{
	private const int DefaultEventsPageSize = 12;
	private const int MaximumEventsPageSize = 100;
	private const int DefaultAwardedCardsPageSize = 3;
	private const int MaximumAwardedCardsPageSize = 100;
	[HttpGet]
	public async Task<ActionResult<object>> List([FromQuery] int page = 1, [FromQuery] int pageSize = DefaultEventsPageSize)
	{
		var normalizedPage = Math.Max(page, 1);
		var normalizedPageSize = Math.Clamp(pageSize, 1, MaximumEventsPageSize);
		return Ok(await companyEventsQueryService.List(GetCompanyId(), normalizedPage, normalizedPageSize, HttpContext.RequestAborted));
	}
	[HttpPost]
	public async Task<ActionResult<object>> Create(CreateEventRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Informe o nome do evento.");
		var bingoEvent = await eventConfigurationService.Create(GetCompanyId(), request.Name, request.MarkingMode, HttpContext.RequestAborted);
		return CreatedAtAction(nameof(Get), new { eventId = bingoEvent.Id }, bingoEvent);
	}
	[HttpGet("{eventId:guid}")]
	public async Task<ActionResult<object>> Get(Guid eventId, [FromQuery] int awardedCardsPage = 1, [FromQuery] int awardedCardsPageSize = DefaultAwardedCardsPageSize)
	{
		var normalizedPage = Math.Max(awardedCardsPage, 1);
		var normalizedPageSize = Math.Clamp(awardedCardsPageSize, 1, MaximumAwardedCardsPageSize);
		var result = await companyEventsQueryService.Get(eventId, normalizedPage, normalizedPageSize, HttpContext.RequestAborted);
		return result is null ? NotFound() : Ok(result);
	}
	[HttpPut("{eventId:guid}/card-purchase")]
	public async Task<IActionResult> UpdateCardPurchase(Guid eventId, OpenCardPurchaseRequest request)
	{
		try
		{
			if (!await cardPurchaseService.UpdateLimit(eventId, request.Quantity, HttpContext.RequestAborted)) return NotFound();
			return NoContent();
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}
	[HttpPost("{eventId:guid}/card-purchase/cancel")]
	public async Task<IActionResult> CancelCardPurchase(Guid eventId, CancelCardPurchaseRequest request)
	{
		try
		{
			if (await cardPurchaseService.Cancel(eventId, request.Reason, HttpContext.RequestAborted) is null) return NotFound();
			return NoContent();
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}
	[HttpPost("{eventId:guid}/registration/open")]
	public async Task<IActionResult> OpenRegistration(Guid eventId, OpenRegistrationRequest request)
	{
		try
		{
			return await eventConfigurationService.OpenRegistration(eventId, request.CardsPerParticipant, HttpContext.RequestAborted) ? NoContent() : NotFound();
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}
	[HttpPost("{eventId:guid}/card-purchase/open")]
	public async Task<IActionResult> OpenCardPurchase(Guid eventId, OpenCardPurchaseRequest request)
	{
		try
		{
			return await eventConfigurationService.OpenCardPurchase(eventId, request.Quantity, HttpContext.RequestAborted) ? NoContent() : NotFound();
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
		catch (UnauthorizedAccessException exception)
		{
			return Unauthorized(exception.Message);
		}
	}
	[HttpPost("{eventId:guid}/participants")]
	public async Task<ActionResult<object>> Join(Guid eventId, JoinEventRequest request)
	{
		try
		{
			var registration = await participantRegistrationService.Register(eventId, new EventParticipantRegistrationRequest(request.Name, request.Type, request.EmployeeRegistration, request.ResponsibleEmployeeName, request.CardsQuantity), null, HttpContext.RequestAborted);
			return registration is null
				? NotFound()
				: Ok(new { participantId = registration.ParticipantId, cards = registration.Cards.Select(card => new { card.CardId, card.PublicCode, card.Numbers, card.Status }) });
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}
	private Guid GetCompanyId() => Guid.Parse(User.FindFirstValue("company_id")!);
}
