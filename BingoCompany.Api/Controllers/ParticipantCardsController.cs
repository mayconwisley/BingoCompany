using BingoCompany.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/participant/cards")]
[Authorize]
public sealed class ParticipantCardsController(IParticipantCardsQueryService participantCardsQueryService) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<object>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 5, CancellationToken cancellationToken = default)
	{
		if (!Guid.TryParse(User.FindFirstValue("participant_account_id"), out var accountId)) return Forbid();
		try
		{
			var cards = await participantCardsQueryService.GetCards(accountId, page, pageSize, cancellationToken);
			return Ok(new { activeCards = cards.ActiveCards, history = new { items = cards.HistoryItems, cards.Page, cards.PageSize, cards.TotalItems, cards.TotalPages } });
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			return new EmptyResult();
		}
	}
}
