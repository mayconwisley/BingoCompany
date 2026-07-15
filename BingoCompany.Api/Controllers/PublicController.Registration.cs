using BingoCompany.Api.Contracts;
using BingoCompany.Api.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Api.Controllers;

public sealed partial class PublicController
{
	[HttpPost("{code}/join")]
	[EnableRateLimiting("public-join")]
	public async Task<ActionResult<object>> Join(string code, JoinEventRequest request)
	{
		var bingoEvent = await db.Events.SingleOrDefaultAsync(item => item.PublicCode == code);
		if (bingoEvent is null) return NotFound();
		try
		{
			var registration = await participantRegistrationService.Register(bingoEvent.Id, request, GetParticipantAccountId(), HttpContext.RequestAborted);
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

	private Guid? GetParticipantAccountId() => Guid.TryParse(User.FindFirst("participant_account_id")?.Value, out var accountId) ? accountId : null;

	[HttpPost("{code}/cards/{cardCode}/activate")]
	[EnableRateLimiting("public-join")]
	public async Task<IActionResult> ActivateDigitalCard(string code, string cardCode)
	{
		var bingoEvent = await db.Events.SingleOrDefaultAsync(item => item.PublicCode == code);
		if (bingoEvent is null) return NotFound();
		if (bingoEvent.Status is not (EventStatus.RegistrationOpen or EventStatus.Running)) return Conflict("Este evento não aceita mais ativação de cartelas.");

		var card = await db.Cards.SingleOrDefaultAsync(item => item.EventId == bingoEvent.Id && item.PublicCode == cardCode);
		if (card is null) return NotFound();
		if (card.Type != CardType.Digital) return BadRequest("Apenas cartelas digitais podem ser ativadas por este acesso.");
		var cardAccountId = await db.Participants.Where(item => item.Id == card.ParticipantId).Select(item => item.ParticipantAccountId).SingleOrDefaultAsync();
		if (cardAccountId.HasValue && cardAccountId != GetParticipantAccountId()) return Forbid();
		if (card.Status == CardStatus.Active) return NoContent();
		if (card.Status != CardStatus.Assigned) return Conflict("Esta cartela não está disponível para ativação.");

		card.Activate();
		db.AuditEntries.Add(new AuditEntry(bingoEvent.Id, "Cartela digital ativada", $"Cartela digital {card.PublicCode} ativada pelo participante."));
		await db.SaveChangesAsync(HttpContext.RequestAborted);
		return NoContent();
	}

}
