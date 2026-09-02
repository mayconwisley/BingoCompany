using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record PublicAuditCard(Guid Id, string PublicCode, CardType Type, CardStatus Status, Guid? ParticipantId, DateTimeOffset CreatedAt);
