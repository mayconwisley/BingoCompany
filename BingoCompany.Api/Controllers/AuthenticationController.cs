using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BingoCompany.Domain.Models;
using BingoCompany.Infrastructure.Persistence;
using BingoCompany.Api.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthenticationController(BingoDbContext db, JwtKeyProvider jwtKeyProvider) : ControllerBase
{
    private readonly PasswordHasher<CompanyUser> _passwordHasher = new();

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterCompanyRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CompanyName) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || request.Password.Length < 8) return BadRequest("Informe empresa, nome, e-mail e uma senha de ao menos 8 caracteres.");
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.CompanyUsers.AnyAsync(item => item.Email == email, cancellationToken)) return Conflict("Este e-mail já está cadastrado.");
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
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.CompanyUsers.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return Unauthorized("E-mail ou senha inválidos.");
        var companyName = await db.Companies.Where(item => item.Id == user.CompanyId).Select(item => item.Name).SingleAsync(cancellationToken);
        return Ok(CreateResponse(user, companyName));
    }

    private AuthResponse CreateResponse(CompanyUser user, string companyName)
    {
        var key = jwtKeyProvider.GetKey();
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim("company_id", user.CompanyId.ToString()), new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Email, user.Email) };
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(12), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), user.Name, companyName);
    }
}

public sealed record RegisterCompanyRequest(string CompanyName, string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string Token, string Name, string CompanyName);
