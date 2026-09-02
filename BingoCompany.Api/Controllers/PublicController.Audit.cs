using Microsoft.AspNetCore.Mvc;

namespace BingoCompany.Api.Controllers;

public sealed partial class PublicController
{
	[HttpGet("{code}/audit")]
	public async Task<ActionResult<object>> Audit(string code, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] int? roundSequence = null, CancellationToken cancellationToken = default)
	{
		var result = await publicEventAuditQueryService.Get(code, page, pageSize, roundSequence, cancellationToken);
		return result is null ? NotFound() : Ok(result);
	}
}
