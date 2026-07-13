using BingoCompany.Api.Controllers;
using BingoCompany.Api.Security;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BingoCompany.Tests;

public sealed class AuthenticationControllerSecurityTests
{
    [Fact]
    public async Task Register_StoresAccessTokenOnlyInSecureHttpOnlyCookie()
    {
        var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new BingoDbContext(options);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["BingoJwtKey"] = "a-secure-test-key-with-at-least-thirty-two-bytes" })
            .Build();
        var authenticationConfiguration = new AuthenticationConfiguration { CookieName = "__Secure-bingo-company-auth", CookiePath = "/bingo" };
        var controller = new AuthenticationController(
            db,
            new JwtKeyProvider(configuration, new TestHostEnvironment()),
            authenticationConfiguration,
            new TestHostEnvironment())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.Register(new RegisterCompanyRequest("Empresa", "Ana", "ana@empresa.test", "uma-senha-segura"), CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<AuthResponse>(response.Value);
        var setCookie = controller.Response.Headers.SetCookie.ToString();

        Assert.Equal("Ana", session.Name);
        Assert.Equal("Empresa", session.CompanyName);
        Assert.DoesNotContain("token", System.Text.Json.JsonSerializer.Serialize(session), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("__Secure-bingo-company-auth=", setCookie, StringComparison.Ordinal);
        Assert.Contains("path=/bingo", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
    }
}
