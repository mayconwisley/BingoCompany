using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface ICardPurchaseWaitlistService
{
	Task<CardPurchaseWaitlistResult?> Join(Guid participantAccountId, string publicCode, int requestedQuantity, CancellationToken cancellationToken);
}
