namespace BingoCompany.Application.Services;

public sealed record RoundConfigurationRequest(string Name, IReadOnlyCollection<RoundStageDefinition> Stages);
