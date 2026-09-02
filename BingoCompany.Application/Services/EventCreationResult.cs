using BingoCompany.Domain.Enums;

namespace BingoCompany.Application.Services;

public sealed record EventCreationResult(Guid Id, string Name, string PublicCode, EventStatus Status, CardMarkingMode MarkingMode, bool IsCardPurchaseOpen, DateTimeOffset CreatedAt);
