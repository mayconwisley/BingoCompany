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

[ApiController, Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthenticationController(IAuthenticationService authenticationService, JwtKeyProvider jwtKeyProvider, AuthenticationConfiguration configuration, IHostEnvironment environment) : ControllerBase
{
	[HttpPost("register")]
	public async Task<ActionResult<AuthResponse>> Register(RegisterCompanyRequest request, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.CompanyName) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || request.Password is null) return BadRequest("Informe empresa, nome, e-mail e uma senha segura.");
		try
		{
			var user = await authenticationService.RegisterCompany(request.CompanyName, request.Name, request.Email, request.Password, cancellationToken);
			return Ok(CreateResponse(user));
		}
		catch (InvalidOperationException exception)
		{
			return BadRequest(exception.Message);
		}
	}

	[HttpPost("login")]
	public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.Email) || request.Password is null) return Unauthorized("E-mail ou senha inválidos.");

		var user = await authenticationService.LoginCompany(request.Email, request.Password, cancellationToken);
		return user is null ? Unauthorized("E-mail ou senha inválidos.") : Ok(CreateResponse(user));
	}

	[HttpPost("logout")]
	public IActionResult Logout()
	{
		Response.Cookies.Delete(configuration.CookieName, new CookieOptions { Path = configuration.CookiePath, Secure = !environment.IsDevelopment(), HttpOnly = true, SameSite = SameSiteMode.Strict });
		return NoContent();
	}

	[HttpGet("session"), Microsoft.AspNetCore.Authorization.Authorize]
	public async Task<ActionResult<AuthenticatedSessionResponse>> GetSession(CancellationToken cancellationToken)
	{
		var name = User.FindFirstValue(ClaimTypes.Name);
		if (string.IsNullOrWhiteSpace(name)) return Unauthorized();

		var participantAccountId = User.FindFirstValue("participant_account_id");
		if (!string.IsNullOrWhiteSpace(participantAccountId))
		{
			return Ok(new AuthenticatedSessionResponse(name, string.Empty, "participant"));
		}

		var companyIdValue = User.FindFirstValue("company_id");
		if (!Guid.TryParse(companyIdValue, out var companyId)) return Unauthorized();

		var companyName = await authenticationService.GetCompanyName(companyId, cancellationToken);
		if (companyName is null) return Unauthorized();

		return Ok(new AuthenticatedSessionResponse(name, companyName, "company"));
	}

	private AuthResponse CreateResponse(CompanyAuthenticationResult user)
	{
		var key = jwtKeyProvider.GetKey();
		var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()), new Claim("company_id", user.CompanyId.ToString()), new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.Email, user.Email) };
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
		return new AuthResponse(user.UserName, user.CompanyName);
	}
}
