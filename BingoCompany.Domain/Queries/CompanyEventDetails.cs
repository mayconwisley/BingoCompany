using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventDetails(Guid Id, string Name, string PublicCode, EventStatus Status, int CardsPerParticipant, bool IsCardPurchaseOpen, int? CardPurchaseLimit, string? CardPurchaseCancellationReason, int? CardPurchaseRemaining, int Participants, int Cards, IReadOnlyCollection<CompanyEventParticipant> ParticipantList, IReadOnlyCollection<CompanyEventCard> CardList, IReadOnlyCollection<CompanyEventAwardedCard> AwardedCards, IReadOnlyCollection<CompanyEventRound> Rounds);
