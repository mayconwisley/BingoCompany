namespace BingoCompany.Domain.Queries;

public sealed record PublicAuditEntriesPage(
	IReadOnlyCollection<PublicAuditEntry> Items,
	int Page,
	int PageSize,
	int TotalItems,
	int TotalPages);
