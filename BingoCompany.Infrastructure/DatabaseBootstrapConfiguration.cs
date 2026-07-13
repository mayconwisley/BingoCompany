using Microsoft.Extensions.Configuration;

namespace BingoCompany.Infrastructure;

internal static class DatabaseBootstrapConfiguration
{
    public static bool ShouldEnsureDatabaseExists(IConfiguration configuration)
    {
        return !bool.TryParse(configuration["Database:EnsureDatabaseExists"], out var ensureDatabaseExists)
            || ensureDatabaseExists;
    }
}
