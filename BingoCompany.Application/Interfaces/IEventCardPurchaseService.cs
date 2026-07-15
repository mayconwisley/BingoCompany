using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IEventCardPurchaseService
{
	Task<bool> UpdateLimit(Guid eventId, int quantity, CancellationToken cancellationToken);
	Task<CardPurchaseCancellationResult?> Cancel(Guid eventId, string reason, CancellationToken cancellationToken);
}
