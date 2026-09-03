using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventDetails(Guid Id, string Name, string PublicCode, EventStatus Status, int CardsPerParticipant, bool IsCardPurchaseOpen, int? CardPurchaseLimit, int? CardPurchasePerParticipantLimit, DateTimeOffset? CardPurchaseClosesAt, int? CardPurchaseLowStockThreshold, string? CardPurchaseCancellationReason, int? CardPurchaseRemaining, CardPurchaseDashboard? CardPurchaseDashboard, int Participants, int Cards, int EligibleCards, CompanyEventAwardedCardsPage AwardedCards, IReadOnlyCollection<CompanyEventRound> Rounds);
