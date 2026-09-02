namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventsPage(IReadOnlyCollection<CompanyEventListItem> Items, int Page, int PageSize, int TotalItems, int TotalPages);
