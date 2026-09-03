using BingoCompany.Application.Services;
using BingoCompany.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BingoCompany.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddBingoApplication(this IServiceCollection services)
	{
		services.AddSingleton<IBingoRoundGameplayService, BingoRoundGameplayService>();
		services.AddScoped<IEventCardPurchaseService, EventCardPurchaseService>();
		services.AddScoped<ICardPurchaseWaitlistService, CardPurchaseWaitlistService>();
		services.AddScoped<ICardStateQueryService, CardStateQueryService>();
		services.AddScoped<IEventParticipantRegistrationService, EventParticipantRegistrationService>();
		services.AddScoped<ICompanyEventAuthorizationService, CompanyEventAuthorizationService>();
		services.AddScoped<ICompanyEventsQueryService, CompanyEventsQueryService>();
		services.AddScoped<IEventConfigurationService, EventConfigurationService>();
		services.AddScoped<ICardLifecycleService, CardLifecycleService>();
		services.AddScoped<IEventRoundManagementService, EventRoundManagementService>();
		services.AddScoped<IManualCardMarkingService, ManualCardMarkingService>();
		services.AddScoped<IRoundGameplayCoordinator, RoundGameplayCoordinator>();
		services.AddScoped<IPublicEventQueryService, PublicEventQueryService>();
		services.AddScoped<IPublicCardActivationService, PublicCardActivationService>();
		services.AddScoped<IPublicEventAuditQueryService, PublicEventAuditQueryService>();
		services.AddScoped<IParticipantCardsQueryService, ParticipantCardsQueryService>();
		services.AddScoped<IPrintedWinnerValidationService, PrintedWinnerValidationService>();
		services.AddScoped<IPrintedCardRegistrationService, PrintedCardRegistrationService>();
		return services;
	}
}
