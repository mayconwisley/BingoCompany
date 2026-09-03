using BingoCompany.Infrastructure.Persistence;
using BingoCompany.Infrastructure.Persistence.Repositories;
using BingoCompany.Infrastructure.Services;
using BingoCompany.Domain.Repositories;
using BingoCompany.Application.Interfaces;
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
		services.AddScoped<ICardPurchaseWaitlistRepository, CardPurchaseWaitlistRepository>();
		services.AddScoped<IEventParticipantRegistrationRepository, EventParticipantRegistrationRepository>();
		services.AddScoped<ICompanyEventAuthorizationRepository, CompanyEventAuthorizationRepository>();
		services.AddScoped<ICompanyEventsReadRepository, CompanyEventsReadRepository>();
		services.AddScoped<IEventConfigurationRepository, EventConfigurationRepository>();
		services.AddScoped<ICardLifecycleRepository, CardLifecycleRepository>();
		services.AddScoped<IEventRoundManagementRepository, EventRoundManagementRepository>();
		services.AddScoped<IManualCardMarkingRepository, ManualCardMarkingRepository>();
		services.AddScoped<IRoundGameplayRepository, RoundGameplayRepository>();
		services.AddScoped<ICardStateReadRepository, CardStateReadRepository>();
		services.AddScoped<IPublicEventReadRepository, PublicEventReadRepository>();
		services.AddScoped<IPublicCardActivationRepository, PublicCardActivationRepository>();
		services.AddScoped<IPublicEventAuditReadRepository, PublicEventAuditReadRepository>();
		services.AddScoped<IAuthenticationService, AuthenticationService>();
		services.AddScoped<IParticipantCardsRepository, ParticipantCardsRepository>();
		services.AddScoped<IPrintedWinnerValidationRepository, PrintedWinnerValidationRepository>();
		services.AddScoped<IPrintedCardRegistrationRepository, PrintedCardRegistrationRepository>();
		return services;
	}
}
