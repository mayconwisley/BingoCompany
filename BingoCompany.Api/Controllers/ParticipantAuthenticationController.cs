using BingoCompany.Api.Contracts;
using BingoCompany.Api.Security;
using BingoCompany.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/participant-auth")]
[EnableRateLimiting("auth")]
public sealed class ParticipantAuthenticationController(IAuthenticationService authenticationService, JwtKeyProvider jwtKeyProvider, AuthenticationConfiguration configuration, IHostEnvironment environment) : ControllerBase
{
	[HttpPost("register")]
	public async Task<ActionResult<ParticipantAuthResponse>> Register(RegisterParticipantAccountRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var account = await authenticationService.RegisterParticipant(request.Name, request.Email, request.Password, cancellationToken);
			return Ok(CreateResponse(account));
		}
		catch (InvalidOperationException exception)
		{
			return BadRequest(exception.Message);
		}
	}

	[HttpPost("login")]
	public async Task<ActionResult<ParticipantAuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
	{
		var account = await authenticationService.LoginParticipant(request.Email, request.Password, cancellationToken);
		return account is null ? Unauthorized("E-mail ou senha inválidos.") : Ok(CreateResponse(account));
	}

	private ParticipantAuthResponse CreateResponse(ParticipantAuthenticationResult account)
	{
		var expiresAt = DateTime.UtcNow.AddMinutes(configuration.TokenLifetimeMinutes);
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, account.AccountId.ToString()),
			new Claim("participant_account_id", account.AccountId.ToString()),
			new Claim("account_type", "participant"),
			new Claim(ClaimTypes.Name, account.Name),
			new Claim(ClaimTypes.Email, account.Email)
		};
		var token = new JwtSecurityToken(configuration.Issuer, configuration.Audience, claims, expires: expiresAt, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyProvider.GetKey())), SecurityAlgorithms.HmacSha256));
		Response.Cookies.Append(configuration.CookieName, new JwtSecurityTokenHandler().WriteToken(token), new CookieOptions { HttpOnly = true, Secure = !environment.IsDevelopment(), SameSite = SameSiteMode.Strict, Path = configuration.CookiePath, Expires = new DateTimeOffset(expiresAt) });
		return new ParticipantAuthResponse(account.Name);
	}
}
