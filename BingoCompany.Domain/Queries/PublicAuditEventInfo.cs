using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record PublicAuditEventInfo(string Name, string PublicCode, EventStatus Status, DateTimeOffset CreatedAt);
