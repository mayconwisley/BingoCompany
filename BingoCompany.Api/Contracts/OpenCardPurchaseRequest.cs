namespace BingoCompany.Api.Contracts;

public sealed record OpenCardPurchaseRequest(int Quantity, int? PerParticipantLimit = null, DateTimeOffset? ClosesAt = null, int? LowStockThreshold = null);
