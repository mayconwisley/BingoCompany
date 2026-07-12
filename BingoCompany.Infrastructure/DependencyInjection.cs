using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BingoCompany.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBingoInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var configuredConnectionString = configuration.GetConnectionString("Bingo")
            ?? throw new InvalidOperationException("A connection string 'Bingo' não foi configurada.");
        var username = Environment.GetEnvironmentVariable("DBBingoUser")
            ?? throw new InvalidOperationException("A variável de ambiente DBBingoUser não foi configurada.");
        var password = Environment.GetEnvironmentVariable("DBBingoPass")
            ?? throw new InvalidOperationException("A variável de ambiente DBBingoPass não foi configurada.");
        var connectionString = new NpgsqlConnectionStringBuilder(configuredConnectionString)
        {
            Username = username,
            Password = password
        }.ConnectionString;

        services.AddDbContext<BingoDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
