using BingoCompany.Application.Services;
using BingoCompany.Domain.Enums;

namespace BingoCompany.Application.Interfaces;

public interface IEventConfigurationService
{
	Task<EventCreationResult> Create(Guid companyId, string name, CardMarkingMode markingMode, CancellationToken cancellationToken);
	Task<bool> OpenRegistration(Guid eventId, int cardsPerParticipant, CancellationToken cancellationToken);
	Task<bool> OpenCardPurchase(Guid eventId, int quantity, CancellationToken cancellationToken);
}
