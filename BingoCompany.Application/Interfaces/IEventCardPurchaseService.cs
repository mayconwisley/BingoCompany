using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IEventCardPurchaseService
{
	Task<bool> UpdateSettings(Guid eventId, int quantity, int perParticipantLimit, DateTimeOffset? closesAt, int lowStockThreshold, CancellationToken cancellationToken);
	Task<CardPurchaseCancellationResult?> Cancel(Guid eventId, string reason, CancellationToken cancellationToken);
	Task<CardPurchaseInvitationResult?> CreateInvitation(Guid eventId, int bonusCards, DateTimeOffset? expiresAt, CancellationToken cancellationToken);
}
