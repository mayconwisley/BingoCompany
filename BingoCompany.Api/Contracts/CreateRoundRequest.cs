namespace BingoCompany.Api.Contracts;

public sealed record CreateRoundRequest(string Name, IReadOnlyList<CreatePrizeStageRequest> Stages);
