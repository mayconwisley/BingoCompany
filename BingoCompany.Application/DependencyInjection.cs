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
		services.AddScoped<IParticipantCardsQueryService, ParticipantCardsQueryService>();
		services.AddScoped<IPrintedWinnerValidationService, PrintedWinnerValidationService>();
		services.AddScoped<IPrintedCardRegistrationService, PrintedCardRegistrationService>();
		return services;
	}
}
