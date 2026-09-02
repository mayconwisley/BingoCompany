using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventListItem(Guid Id, string Name, string PublicCode, EventStatus Status, CardMarkingMode MarkingMode, bool IsCardPurchaseOpen, DateTimeOffset CreatedAt);
