using System.Security.Claims;
using System.Text.Json;
using BingoCompany.Api.Controllers;
using BingoCompany.Domain;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class EventsControllerPaginationTests
{
	[Fact]
	public async Task List_ReturnsOrderedPageWithCreationDateForTheAuthenticatedCompany()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var companyId = Guid.CreateVersion7();
		var firstEvent = new BingoEvent(companyId, "Primeiro");
		var secondEvent = new BingoEvent(companyId, "Segundo");
		var thirdEvent = new BingoEvent(companyId, "Terceiro");
		db.Events.AddRange(firstEvent, secondEvent, thirdEvent, new BingoEvent(Guid.CreateVersion7(), "Outro evento"));
		await db.SaveChangesAsync();
		var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("company_id", companyId.ToString())])) };
		var controller = new EventsController(db, null!, null!, null!, null!, null!, null!) { ControllerContext = new ControllerContext { HttpContext = httpContext } };

		var response = Assert.IsType<OkObjectResult>((await controller.List(1, 2)).Result);
		using var result = JsonDocument.Parse(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

		Assert.Equal(1, result.RootElement.GetProperty("page").GetInt32());
		Assert.Equal(2, result.RootElement.GetProperty("pageSize").GetInt32());
		Assert.Equal(3, result.RootElement.GetProperty("totalItems").GetInt32());
		Assert.Equal(2, result.RootElement.GetProperty("totalPages").GetInt32());
		var items = result.RootElement.GetProperty("items");
		Assert.Equal(2, items.GetArrayLength());
		Assert.Equal("Terceiro", items[0].GetProperty("name").GetString());
		Assert.True(items[0].TryGetProperty("createdAt", out var createdAt));
		Assert.Equal(JsonValueKind.String, createdAt.ValueKind);
	}
}
