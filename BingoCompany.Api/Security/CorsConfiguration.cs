namespace BingoCompany.Api.Security;

public sealed class CorsConfiguration
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}
