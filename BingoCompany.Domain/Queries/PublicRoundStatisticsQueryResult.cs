namespace BingoCompany.Domain.Queries;

public sealed record PublicRoundStatisticsQueryResult(int TotalCards, int OneNumberAway, int TwoNumbersAway, int ThreeNumbersAway, int AwardedCards);
