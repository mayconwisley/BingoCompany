namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventAwardedCardsPage(IReadOnlyCollection<CompanyEventAwardedCard> Items, int Page, int PageSize, int TotalItems, int TotalPages);
