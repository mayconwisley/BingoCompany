namespace BingoCompany.Api.Contracts;

public sealed record CreateCardPurchaseInvitationRequest(int BonusCards, DateTimeOffset? ExpiresAt = null);
