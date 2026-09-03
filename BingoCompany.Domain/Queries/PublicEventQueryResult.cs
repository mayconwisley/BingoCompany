using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record PublicEventQueryResult(
	Guid Id,
	string Name,
	string PublicCode,
	EventStatus Status,
	CardMarkingMode MarkingMode,
	bool IsCardPurchaseOpen,
	int? CardPurchaseLimit,
	int? CardPurchasePerParticipantLimit,
	DateTimeOffset? CardPurchaseClosesAt,
	int? CardPurchaseLowStockThreshold,
	string? CardPurchaseCancellationReason,
	int? CardPurchaseRemaining,
	int CardPurchaseWaitlistEntries,
	int Participants,
	int Cards,
	PublicRoundQueryResult? Round);
