using BingoCompany.Api.Security;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class CompanyEventOwnerFilterTests
{
	[Fact]
	public async Task OnActionExecutionAsync_AllowsAnonymousCardStateEndpointWithoutCompanyClaim()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var actionDescriptor = new ActionDescriptor { EndpointMetadata = [new AllowAnonymousAttribute()] };
		var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(new RouteValueDictionary { ["eventId"] = Guid.CreateVersion7() }), actionDescriptor);
		var actionExecutingContext = new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
		var filter = new CompanyEventOwnerFilter(db);
		var executed = false;

		await filter.OnActionExecutionAsync(actionExecutingContext, () =>
		{
			executed = true;
			return Task.FromResult(new ActionExecutedContext(actionContext, [], new object()));
		});

		Assert.True(executed);
		Assert.Null(actionExecutingContext.Result);
	}
}
