namespace BingoCompany.Application.Services;

public sealed record CardPurchaseInvitationResult(string Code, int BonusCards, DateTimeOffset? ExpiresAt);
