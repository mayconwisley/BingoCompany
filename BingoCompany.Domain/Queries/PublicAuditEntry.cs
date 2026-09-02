namespace BingoCompany.Domain.Queries;

public sealed record PublicAuditEntry(string Action, string Details, DateTimeOffset OccurredAt);
