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
			var registration = await participantRegistrationService.Register(bingoEvent.Id, request, HttpContext.RequestAborted);
			return registration is null
				? NotFound()
				: Ok(new { participantId = registration.ParticipantId, cardId = registration.CardId, registration.PublicCode, registration.Numbers });
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}

}
