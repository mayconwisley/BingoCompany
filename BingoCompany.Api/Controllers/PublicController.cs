using BingoCompany.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/public/events")]
public sealed partial class PublicController(IEventParticipantRegistrationService participantRegistrationService, IPublicEventQueryService publicEventQueryService, IPublicCardActivationService publicCardActivationService, IPublicEventAuditQueryService publicEventAuditQueryService) : ControllerBase
{
	[HttpGet("{code}")]
	public async Task<ActionResult<object>> Get(string code, CancellationToken cancellationToken = default)
	{
		var result = await publicEventQueryService.Get(code, cancellationToken);
		return result is null ? NotFound() : Ok(result);
	}
}
