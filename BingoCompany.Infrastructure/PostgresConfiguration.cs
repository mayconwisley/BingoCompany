using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BingoCompany.Infrastructure;

internal static class PostgresConfiguration
{
    public static string CreateConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Bingo")
            ?? throw new InvalidOperationException("A connection string 'Bingo' não foi configurada.");
        var username = GetEnvironmentVariable("DBBingoUser")
            ?? throw new InvalidOperationException("A variável de ambiente DBBingoUser não foi configurada.");
        var password = GetEnvironmentVariable("DBBingoPass")
            ?? throw new InvalidOperationException("A variável de ambiente DBBingoPass não foi configurada.");

        return connectionString
            .Replace("{{username}}", username)
            .Replace("{{password}}", password);
    }

    public static void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.CommandTimeout(30);
            postgres.EnableRetryOnFailure(
                maxRetryCount: 2,
                maxRetryDelay: TimeSpan.FromSeconds(2),
                errorCodesToAdd: null);
            postgres.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        });
    }

    private static string? GetEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return OperatingSystem.IsWindows()
            ? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine)
            : null;
    }
}
