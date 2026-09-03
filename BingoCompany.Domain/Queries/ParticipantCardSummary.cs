using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record ParticipantCardSummary(Guid EventId, string EventPublicCode, string EventName, EventStatus EventStatus, string PublicCode, CardStatus Status, bool HasParticipatedInRound, bool IsEligibleForNextRound, string? InvalidationReason, string? EventCancellationReason, DateTimeOffset CreatedAt);
