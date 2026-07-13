namespace BingoCompany.Api.Security;

public sealed class AuthenticationConfiguration
{
    public const string SectionName = "Authentication";

    public string CookieName { get; init; } = "bingo-company-auth";
    public string Issuer { get; init; } = "BingoCompany.Api";
    public string Audience { get; init; } = "BingoCompany.Frontend";
    public int TokenLifetimeMinutes { get; init; } = 120;
    public string CookiePath { get; init; } = "/";
}
