namespace BingoCompany.Domain.Queries;

public sealed record PublicAuditParticipant(Guid Id, string Name, DateTimeOffset JoinedAt);
