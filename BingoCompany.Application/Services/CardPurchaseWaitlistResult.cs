namespace BingoCompany.Application.Services;

public sealed record CardPurchaseWaitlistResult(int Position, int RequestedQuantity, bool AlreadyRegistered);
