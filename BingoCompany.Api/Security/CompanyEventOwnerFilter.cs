using System.Security.Claims;
using BingoCompany.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BingoCompany.Api.Security;

public sealed class CompanyEventOwnerFilter(ICompanyEventAuthorizationService authorizationService) : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
		{
			await next();
			return;
		}

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

		var ownsEvent = await authorizationService.OwnsEvent(eventId, parsedCompanyId, context.HttpContext.RequestAborted);
		if (!ownsEvent)
		{
			context.Result = new NotFoundResult();
			return;
		}

		await next();
	}
}
