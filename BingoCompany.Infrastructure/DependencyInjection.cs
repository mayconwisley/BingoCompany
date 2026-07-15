using BingoCompany.Infrastructure.Persistence;
using BingoCompany.Infrastructure.Persistence.Repositories;
using BingoCompany.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BingoCompany.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddBingoInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		if (DatabaseBootstrapConfiguration.ShouldEnsureDatabaseExists(configuration))
		{
			PostgresDatabaseBootstrapper.EnsureDatabaseExists(configuration);
		}
		var connectionString = PostgresConfiguration.CreateConnectionString(configuration);
		var poolSize = int.TryParse(configuration["Database:DbContextPoolSize"], out var configuredPoolSize)
			? Math.Max(configuredPoolSize, 1)
			: 128;

		services.AddDbContextPool<BingoDbContext>(options => PostgresConfiguration.Configure(options, connectionString), poolSize);
		services.AddScoped<IEventCardPurchaseRepository, EventCardPurchaseRepository>();
		services.AddScoped<IParticipantCardsRepository, ParticipantCardsRepository>();
		services.AddScoped<IPrintedWinnerValidationRepository, PrintedWinnerValidationRepository>();
		services.AddScoped<IPrintedCardRegistrationRepository, PrintedCardRegistrationRepository>();
		return services;
	}
}
