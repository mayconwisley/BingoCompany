using BingoCompany.Api.Contracts;
using BingoCompany.Api.Security;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/participant-auth")]
[EnableRateLimiting("auth")]
public sealed class ParticipantAuthenticationController(BingoDbContext db, JwtKeyProvider jwtKeyProvider, AuthenticationConfiguration configuration, IHostEnvironment environment) : ControllerBase
{
	private readonly PasswordHasher<ParticipantAccount> _passwordHasher = new();

	[HttpPost("register")]
	public async Task<ActionResult<ParticipantAuthResponse>> Register(RegisterParticipantAccountRequest request, CancellationToken cancellationToken)
	{
		var email = request.Email.Trim().ToLowerInvariant();
		if (await db.ParticipantAccounts.AnyAsync(item => item.Email == email, cancellationToken)) return BadRequest("Já existe uma conta de participante com este e-mail.");

		var account = new ParticipantAccount(request.Name, email, string.Empty);
		account = new ParticipantAccount(request.Name, email, _passwordHasher.HashPassword(account, request.Password));
		db.ParticipantAccounts.Add(account);
		await db.SaveChangesAsync(cancellationToken);
		return Ok(CreateResponse(account));
	}

	[HttpPost("login")]
	public async Task<ActionResult<ParticipantAuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
	{
		var email = request.Email.Trim().ToLowerInvariant();
		var account = await db.ParticipantAccounts.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);
		if (account is null || _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return Unauthorized("E-mail ou senha inválidos.");

		return Ok(CreateResponse(account));
	}

	private ParticipantAuthResponse CreateResponse(ParticipantAccount account)
	{
		var expiresAt = DateTime.UtcNow.AddMinutes(configuration.TokenLifetimeMinutes);
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
			new Claim("participant_account_id", account.Id.ToString()),
			new Claim("account_type", "participant"),
			new Claim(ClaimTypes.Name, account.Name),
			new Claim(ClaimTypes.Email, account.Email)
		};
		var token = new JwtSecurityToken(configuration.Issuer, configuration.Audience, claims, expires: expiresAt, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyProvider.GetKey())), SecurityAlgorithms.HmacSha256));
		Response.Cookies.Append(configuration.CookieName, new JwtSecurityTokenHandler().WriteToken(token), new CookieOptions { HttpOnly = true, Secure = !environment.IsDevelopment(), SameSite = SameSiteMode.Strict, Path = configuration.CookiePath, Expires = new DateTimeOffset(expiresAt) });
		return new ParticipantAuthResponse(account.Name);
	}
}
