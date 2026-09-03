using BingoCompany.Api.Contracts;
using BingoCompany.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BingoCompany.Api.Controllers;

public sealed partial class PublicController
{
	[HttpPost("{code}/join")]
	[EnableRateLimiting("public-join")]
	public async Task<ActionResult<object>> Join(string code, JoinEventRequest request)
	{
		try
		{
			var registration = await participantRegistrationService.RegisterByPublicCode(code, new EventParticipantRegistrationRequest(request.Name, request.Type, request.EmployeeRegistration, request.ResponsibleEmployeeName, request.CardsQuantity, request.InvitationCode), GetParticipantAccountId(), HttpContext.RequestAborted);
			return registration is null
				? NotFound()
				: Ok(new { participantId = registration.ParticipantId, cards = registration.Cards.Select(card => new { card.CardId, card.PublicCode, card.Numbers, card.Status }) });
		}
		catch (UnauthorizedAccessException exception)
		{
			return Unauthorized(exception.Message);
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

	[HttpPost("{code}/card-purchase/waitlist")]
	[EnableRateLimiting("public-join")]
	public async Task<ActionResult<object>> JoinCardPurchaseWaitlist(string code, JoinCardPurchaseWaitlistRequest request)
	{
		var participantAccountId = GetParticipantAccountId();
		if (!participantAccountId.HasValue) return Unauthorized("Entre com sua conta de participante para entrar na lista de espera.");
		try
		{
			var result = await (cardPurchaseWaitlistService ?? throw new InvalidOperationException("Serviço de lista de espera não configurado.")).Join(participantAccountId.Value, code, request.RequestedQuantity, HttpContext.RequestAborted);
			return result is null ? NotFound() : Ok(result);
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

	private Guid? GetParticipantAccountId() => Guid.TryParse(User.FindFirst("participant_account_id")?.Value, out var accountId) ? accountId : null;

	[HttpPost("{code}/cards/{cardCode}/activate")]
	[EnableRateLimiting("public-join")]
	public async Task<IActionResult> ActivateDigitalCard(string code, string cardCode)
	{
		try
		{
			var result = await publicCardActivationService.Activate(code, cardCode, GetParticipantAccountId(), HttpContext.RequestAborted);
			return result is DigitalCardActivationResult.EventNotFound or DigitalCardActivationResult.CardNotFound ? NotFound() : NoContent();
		}
		catch (UnauthorizedAccessException)
		{
			return Forbid();
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

}
