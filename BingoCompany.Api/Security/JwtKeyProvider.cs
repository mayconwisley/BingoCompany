namespace BingoCompany.Api.Security;

public sealed class JwtKeyProvider(IConfiguration configuration)
{
    public string GetKey() => configuration["BingoJwtKey"]
        ?? configuration["Authentication:JwtKey"]
        ?? throw new InvalidOperationException("Configure a variável de ambiente BingoJwtKey para assinar os tokens de acesso.");
}
