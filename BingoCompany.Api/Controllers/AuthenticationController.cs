using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using BingoCompany.Domain.Models;
using BingoCompany.Infrastructure.Persistence;
using BingoCompany.Api.Security;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthenticationController(BingoDbContext db, JwtKeyProvider jwtKeyProvider, AuthenticationConfiguration configuration, IHostEnvironment environment) : ControllerBase
{
    private readonly PasswordHasher<CompanyUser> _passwordHasher = new();

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterCompanyRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CompanyName) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || request.Password is null) return BadRequest("Informe empresa, nome, e-mail e uma senha segura.");
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.CompanyUsers.AnyAsync(item => item.Email == email, cancellationToken)) return BadRequest("Não foi possível concluir o cadastro com os dados informados.");
        var company = new Company(request.CompanyName.Trim());
        var user = new CompanyUser(company.Id, request.Name.Trim(), email, string.Empty);
        user = new CompanyUser(company.Id, request.Name.Trim(), email, _passwordHasher.HashPassword(user, request.Password));
        db.Companies.Add(company);
        db.CompanyUsers.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(CreateResponse(user, company.Name));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || request.Password is null) return Unauthorized("E-mail ou senha inválidos.");

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.CompanyUsers.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (user is null) return Unauthorized("E-mail ou senha inválidos.");

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed) return Unauthorized("E-mail ou senha inválidos.");

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.UpdatePasswordHash(_passwordHasher.HashPassword(user, request.Password));
            await db.SaveChangesAsync(cancellationToken);
        }

        var companyName = await db.Companies.Where(item => item.Id == user.CompanyId).Select(item => item.Name).SingleAsync(cancellationToken);
        return Ok(CreateResponse(user, companyName));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(configuration.CookieName, new CookieOptions { Path = configuration.CookiePath, Secure = !environment.IsDevelopment(), HttpOnly = true, SameSite = SameSiteMode.Strict });
        return NoContent();
    }

    private AuthResponse CreateResponse(CompanyUser user, string companyName)
    {
        var key = jwtKeyProvider.GetKey();
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim("company_id", user.CompanyId.ToString()), new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Email, user.Email) };
        var expiresAt = DateTime.UtcNow.AddMinutes(configuration.TokenLifetimeMinutes);
        var token = new JwtSecurityToken(configuration.Issuer, configuration.Audience, claims, expires: expiresAt, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        Response.Cookies.Append(configuration.CookieName, new JwtSecurityTokenHandler().WriteToken(token), new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = configuration.CookiePath,
            Expires = new DateTimeOffset(expiresAt)
        });
        return new AuthResponse(user.Name, companyName);
    }
}

public sealed record RegisterCompanyRequest(
    [property: Required, StringLength(120, MinimumLength = 2)] string CompanyName,
    [property: Required, StringLength(120, MinimumLength = 2)] string Name,
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, StringLength(128, MinimumLength = 12)] string Password);

public sealed record LoginRequest(
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, StringLength(128)] string Password);

public sealed record AuthResponse(string Name, string CompanyName);
