using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class RoundGameplayCoordinator(IRoundGameplayRepository repository, IBingoRoundGameplayService gameplayService) : IRoundGameplayCoordinator
{
	public async Task<RoundDrawOperationResult?> Draw(Guid eventId, Guid roundId, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		var round = await repository.GetRound(eventId, roundId, cancellationToken);
		if (bingoEvent is null || round is null) return null;
		var eligibleCards = await repository.GetEligibleCards(roundId, cancellationToken);
		var cards = bingoEvent.MarkingMode == CardMarkingMode.Automatic ? eligibleCards : eligibleCards.Where(card => card.Type == CardType.Digital).ToArray();
		var marks = bingoEvent.MarkingMode == CardMarkingMode.Automatic ? [] : await repository.GetMarks(roundId, cancellationToken);
		var drawResult = gameplayService.Draw(round, cards, marks, bingoEvent.MarkingMode, await repository.GetExcludedCardIds(roundId, round.ActiveStage.Id, cancellationToken));
		repository.AddDrawnNumber(drawResult.DrawnNumber); repository.AddWinners(drawResult.Winners);
		repository.AddAuditEntry(new AuditEntry(eventId, "Pedra sorteada", $"Rodada {round.Name}: pedra {drawResult.DrawnNumber.Number} na posição {drawResult.DrawnNumber.Sequence}."));
		if (drawResult.HasWinners) repository.AddAuditEntry(new AuditEntry(eventId, "Prêmio detectado", $"{drawResult.Winners.Count} cartela(s) atingiram {round.ActiveStage.PrizeName}."));
		await repository.SaveChanges(cancellationToken);
		return new RoundDrawOperationResult(roundId, drawResult.DrawnNumber.Number, drawResult.DrawnNumber.Sequence, drawResult.Winners.Count, drawResult.RequiresTieBreaker);
	}

	public async Task<WinnerRevealOperationResult> Reveal(Guid eventId, Guid roundId, CancellationToken cancellationToken)
	{
		var round = await repository.GetRound(eventId, roundId, cancellationToken);
		if (round is null || round.Status is not (RoundStatus.WinnerDetected or RoundStatus.TieBreaker)) throw new InvalidOperationException("Não há vencedor aguardando revelação.");
		var candidates = await repository.GetCandidates(roundId, round.ActiveStage.Id, cancellationToken);
		if (candidates.Count == 0) throw new KeyNotFoundException();
		var requiresRecovery = candidates.Any(candidate => candidate.RevealedAt.HasValue) && candidates.All(candidate => !candidate.IsWinner);
		if (candidates.Any(candidate => candidate.RevealedAt.HasValue) && !requiresRecovery) throw new InvalidOperationException("O vencedor desta etapa já foi revelado.");
		var winner = gameplayService.RevealWinner(round, candidates, DateTimeOffset.UtcNow).Winner;
		repository.AddAuditEntry(new AuditEntry(eventId, requiresRecovery ? "Vencedor recuperado" : candidates.Count > 1 ? "Desempate concluído" : "Prêmio revelado", $"Prêmio {round.Stages.Single(stage => stage.Id == winner.StageId).PrizeName} revelado."));
		if (!await repository.TrySaveChanges(cancellationToken)) throw new InvalidOperationException("O vencedor desta etapa já foi revelado.");
		var names = await repository.GetParticipantNames(candidates.Select(candidate => candidate.ParticipantId).ToArray(), cancellationToken);
		var card = await repository.GetCard(winner.CardId, cancellationToken) ?? throw new InvalidOperationException("Cartela vencedora não encontrada.");
		return new WinnerRevealOperationResult(names[winner.ParticipantId], card.PublicCode, round.Stages.Single(stage => stage.Id == winner.StageId).PrizeName, candidates.Select(candidate => new WinnerTieBreakerResult(names[candidate.ParticipantId], candidate.TieBreakerNumber, candidate.IsWinner)).ToArray(), round.Stages.SingleOrDefault(stage => stage.IsActive)?.PrizeName, round.Status);
	}

	public async Task<PrizeStageOperationResult?> MarkDelivered(Guid eventId, Guid roundId, CancellationToken cancellationToken)
	{
		var round = await repository.GetRound(eventId, roundId, cancellationToken); if (round is null) return null;
		var winner = await repository.GetPendingWinner(roundId, round.ActiveStage.Id, cancellationToken) ?? throw new InvalidOperationException("Não há prêmio aguardando confirmação de entrega.");
		winner.MarkPrizeDelivered(DateTimeOffset.UtcNow); round.FinishStage();
		repository.AddAuditEntry(new AuditEntry(eventId, "Prêmio entregue", $"O prêmio {round.Stages.Single(stage => stage.Id == winner.StageId).PrizeName} foi entregue ao vencedor."));
		if (round.Status == RoundStatus.Finished) repository.AddAuditEntry(new AuditEntry(eventId, "Rodada encerrada", $"Rodada {round.Name} encerrada."));
		await repository.SaveChanges(cancellationToken); return CreateStageResult(round);
	}

	public async Task<PrizeStageOperationResult?> MarkDeclined(Guid eventId, Guid roundId, CancellationToken cancellationToken)
	{
		var round = await repository.GetRound(eventId, roundId, cancellationToken); if (round is null) return null;
		var winner = await repository.GetPendingWinner(roundId, round.ActiveStage.Id, cancellationToken) ?? throw new InvalidOperationException("Não há prêmio aguardando confirmação de retirada.");
		winner.MarkPrizeDeclined(DateTimeOffset.UtcNow); round.ResumeDrawingAfterPrizeDeclined();
		repository.AddAuditEntry(new AuditEntry(eventId, "Prêmio não retirado", $"O vencedor do prêmio {round.ActiveStage.PrizeName} não retirou o prêmio; o sorteio continuará com a mesma regra."));
		await repository.SaveChanges(cancellationToken); return CreateStageResult(round);
	}

	public async Task<PrizeStageOperationResult?> ClosePresentation(Guid eventId, Guid roundId, CancellationToken cancellationToken)
	{
		var round = await repository.GetRound(eventId, roundId, cancellationToken); if (round is null) return null;
		round.CloseWinnerPresentation(); await repository.SaveChanges(cancellationToken); return CreateStageResult(round);
	}

	private static PrizeStageOperationResult CreateStageResult(BingoRound round) => new(round.Id, round.Stages.SingleOrDefault(stage => stage.IsActive)?.PrizeName, round.Status);
}
