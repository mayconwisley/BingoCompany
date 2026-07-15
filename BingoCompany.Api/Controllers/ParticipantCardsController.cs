using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/participant/cards")]
[Authorize]
public sealed class ParticipantCardsController(BingoDbContext db) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<object>> List(CancellationToken cancellationToken)
	{
		if (!Guid.TryParse(User.FindFirstValue("participant_account_id"), out var accountId)) return Forbid();
		try
		{
			var cards = await (
				from card in db.Cards.AsNoTracking()
				join participant in db.Participants.AsNoTracking() on card.ParticipantId equals participant.Id
				join bingoEvent in db.Events.AsNoTracking() on card.EventId equals bingoEvent.Id
				where participant.ParticipantAccountId == accountId
				orderby bingoEvent.CreatedAt descending, card.CreatedAt descending
				select new { eventId = bingoEvent.Id, eventPublicCode = bingoEvent.PublicCode, eventName = bingoEvent.Name, eventStatus = bingoEvent.Status, card.PublicCode, card.Status, card.InvalidationReason, eventCancellationReason = bingoEvent.CardPurchaseCancellationReason, card.CreatedAt }
			).ToListAsync(cancellationToken);
			return Ok(cards);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			return new EmptyResult();
		}
	}
}
