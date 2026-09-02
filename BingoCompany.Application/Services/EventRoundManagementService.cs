using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class EventRoundManagementService(IEventRoundManagementRepository repository) : IEventRoundManagementService
{
	private const int MaximumPrizeImageLength = 2_800_000;
	private static readonly string[] SupportedPrizeImagePrefixes = ["data:image/jpeg;base64,", "data:image/png;base64,", "data:image/webp;base64,"];

	public async Task<RoundConfigurationResult?> Create(Guid eventId, RoundConfigurationRequest request, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		if (bingoEvent is null) return null;
		EnsureEventIsMutable(bingoEvent);
		ValidateConfiguration(request);
		var round = new BingoRound(eventId, await repository.CountRounds(eventId, cancellationToken) + 1, request.Name);
		var stages = CreateStages(round.Id, request.Stages);
		foreach (var stage in stages) round.AddStage(stage);
		repository.AddRound(round);
		repository.AddAuditEntry(new AuditEntry(eventId, "Rodada criada", $"Rodada {round.Name} criada com {stages.Length} etapas."));
		await repository.SaveChanges(cancellationToken);
		return new RoundConfigurationResult(round.Id, round.Name);
	}

	public async Task<bool> Update(Guid eventId, Guid roundId, RoundConfigurationRequest request, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		var round = await repository.GetRoundWithStages(eventId, roundId, cancellationToken);
		if (bingoEvent is null || round is null) return false;
		EnsureEventIsMutable(bingoEvent);
		ValidateConfiguration(request);
		var stages = CreateStages(round.Id, request.Stages);
		round.Update(request.Name, stages);
		repository.AddPrizeStages(stages);
		repository.AddAuditEntry(new AuditEntry(eventId, "Rodada editada", $"Rodada {round.Name} atualizada com {stages.Length} etapas."));
		if (!await repository.TrySaveChanges(cancellationToken)) throw new RoundConcurrencyException();
		return true;
	}

	public async Task<StartedRoundResult?> Start(Guid eventId, Guid roundId, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		var round = await repository.GetRoundWithStages(eventId, roundId, cancellationToken);
		if (bingoEvent is null || round is null) return null;
		EnsureEventIsMutable(bingoEvent);
		if (!await repository.HasActiveEligibleCard(eventId, cancellationToken)) throw new InvalidOperationException("Gere e ative ao menos uma cartela associada a participante antes de abrir a operação.");
		if (bingoEvent.Status == EventStatus.RegistrationOpen) bingoEvent.Start();
		round.FreezeEligibility(await repository.GetEventCards(eventId, cancellationToken));
		repository.AddEligibleCards(round.EligibleCards);
		var sequence = SecureDrawSequence.Generate();
		round.Start(sequence, SecureDrawSequence.Hash(sequence));
		repository.AddAuditEntry(new AuditEntry(eventId, "Rodada iniciada", $"Rodada {round.Name} iniciada. Hash {round.SequenceHash}."));
		await repository.SaveChanges(cancellationToken);
		return new StartedRoundResult(round.Id, round.SequenceHash);
	}

	public async Task<CancelledRoundResult?> Cancel(Guid eventId, Guid roundId, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		var round = await repository.GetRoundWithDrawnNumbers(eventId, roundId, cancellationToken);
		if (bingoEvent is null || round is null) return null;
		EnsureEventIsMutable(bingoEvent);
		round.Cancel();
		repository.AddAuditEntry(new AuditEntry(eventId, "Rodada cancelada", $"Rodada {round.Name} cancelada após {round.DrawnNumbers.Count} pedras sorteadas."));
		await repository.SaveChanges(cancellationToken);
		return new CancelledRoundResult(round.Id);
	}

	public async Task<bool> FinishEvent(Guid eventId, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEventWithRounds(eventId, cancellationToken);
		if (bingoEvent is null) return false;
		bingoEvent.Finish();
		var unusedCards = await repository.GetAssignedDigitalCards(eventId, cancellationToken);
		foreach (var card in unusedCards) card.InvalidateUnused();
		repository.AddAuditEntry(new AuditEntry(eventId, "Evento encerrado", "O evento foi encerrado e tornou-se imutável."));
		if (unusedCards.Count > 0) repository.AddAuditEntry(new AuditEntry(eventId, "Cartelas não utilizadas invalidadas", $"{unusedCards.Count} cartela(s) digital(is) não ativada(s) foram invalidadas."));
		await repository.SaveChanges(cancellationToken);
		return true;
	}

	private static void EnsureEventIsMutable(BingoEvent bingoEvent)
	{
		if (bingoEvent.Status == EventStatus.Finished) throw new InvalidOperationException("O evento encerrado não pode ser alterado.");
	}

	private static PrizeStage[] CreateStages(Guid roundId, IReadOnlyCollection<RoundStageDefinition> stages)
	{
		var orderedStages = PrizeStageOrdering.Order(stages, stage => stage.Pattern);
		return orderedStages.Select((stage, index) => new PrizeStage(roundId, index + 1, stage.PrizeName, stage.Pattern, stage.PrizeImageDataUrl)).ToArray();
	}

	private static void ValidateConfiguration(RoundConfigurationRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Informe o nome da rodada.");
		if (request.Stages.Count == 0) throw new ArgumentException("Configure ao menos uma etapa de prêmio.");
		if (request.Stages.Any(stage => string.IsNullOrWhiteSpace(stage.PrizeName))) throw new ArgumentException("Informe o nome de cada prêmio.");
		if (request.Stages.Any(stage => !IsValidPrizeImage(stage.PrizeImageDataUrl))) throw new ArgumentException("A foto do prêmio deve ser uma imagem JPEG, PNG ou WebP de até 2 MB.");
		if (request.Stages.GroupBy(stage => stage.Pattern).Any(group => group.Count() > 1)) throw new ArgumentException("Cada regra de premiação pode ser usada apenas uma vez por rodada.");
	}

	private static bool IsValidPrizeImage(string? imageDataUrl) => string.IsNullOrWhiteSpace(imageDataUrl) || imageDataUrl.Length <= MaximumPrizeImageLength && SupportedPrizeImagePrefixes.Any(prefix => imageDataUrl.StartsWith(prefix, StringComparison.Ordinal));
}
