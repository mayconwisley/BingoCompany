using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventRound(Guid Id, string Name, RoundStatus Status, DateTimeOffset CreatedAt, IReadOnlyCollection<CompanyEventPrizeStage> Stages);
