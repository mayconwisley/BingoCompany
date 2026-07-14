using System.Security.Claims;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Api.Security;

public sealed class CompanyEventOwnerFilter(BingoDbContext db) : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		if (!context.RouteData.Values.TryGetValue("eventId", out var rawEventId) || !Guid.TryParse(rawEventId?.ToString(), out var eventId))
		{
			await next();
			return;
		}

		var companyId = context.HttpContext.User.FindFirstValue("company_id");
		if (!Guid.TryParse(companyId, out var parsedCompanyId))
		{
			context.Result = new ForbidResult();
			return;
		}

		var ownsEvent = await db.Events.AnyAsync(item => item.Id == eventId && item.CompanyId == parsedCompanyId, context.HttpContext.RequestAborted);
		if (!ownsEvent)
		{
			context.Result = new NotFoundResult();
			return;
		}

		await next();
	}
}
