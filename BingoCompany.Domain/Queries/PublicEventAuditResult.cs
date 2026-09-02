namespace BingoCompany.Domain.Queries;

public sealed record PublicEventAuditResult(PublicAuditEventInfo EventInfo, IReadOnlyCollection<PublicAuditParticipant> Participants, IReadOnlyCollection<PublicAuditCard> Cards, IReadOnlyCollection<PublicAuditRound> Rounds, IReadOnlyCollection<PublicAuditEntry> Entries);
