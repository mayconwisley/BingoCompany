using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class PrintedWinnerValidationService(IPrintedWinnerValidationRepository repository) : IPrintedWinnerValidationService
{
	public async Task<PrintedWinnerValidationResult?> Validate(Guid eventId, Guid roundId, string cardCode, CancellationToken cancellationToken)
	{
		var round = await repository.GetRound(eventId, roundId, cancellationToken);
		var card = await repository.GetCard(eventId, cardCode, cancellationToken);
		if (round is null || card is null) return null;
		if (round.Status is not (RoundStatus.Drawing or RoundStatus.WinnerDetected or RoundStatus.TieBreaker)) throw new InvalidOperationException("A rodada não está aceitando conferência de cartelas.");
		if (card.Type != CardType.Printed || !card.IsEligible) throw new InvalidOperationException("Esta cartela impressa não está ativa para a rodada.");
		if (!await repository.IsEligible(roundId, card.Id, cancellationToken)) throw new InvalidOperationException("Esta cartela não participa desta rodada.");
		if (await repository.HasCandidate(roundId, round.ActiveStage.Id, card.Id, cancellationToken)) throw new InvalidOperationException("Esta cartela já foi conferida como candidata nesta etapa.");
		if (!WinningPatternEvaluator.IsCompleted(card.Numbers, round.DrawnNumbers.Select(item => item.Number).ToHashSet(), round.ActiveStage.Pattern)) throw new InvalidOperationException("A cartela não completou a regra do prêmio com as pedras sorteadas.");

		var candidatesCount = await repository.CountCandidates(roundId, round.ActiveStage.Id, cancellationToken) + 1;
		if (round.Status == RoundStatus.Drawing) round.DetectWinner();
		if (candidatesCount > 1 && round.Status == RoundStatus.WinnerDetected) round.StartTieBreaker();
		repository.AddWinner(new RoundWinner(round.Id, round.ActiveStage.Id, card.Id, card.ParticipantId!.Value, round.DrawnNumbers.Count));
		repository.AddAuditEntry(new AuditEntry(eventId, "Cartela impressa conferida", $"Cartela {card.PublicCode} validada para {round.ActiveStage.PrizeName}."));
		await repository.SaveChanges(cancellationToken);
		return new PrintedWinnerValidationResult(await repository.GetParticipantName(card.ParticipantId.Value, cancellationToken), card.PublicCode, candidatesCount, round.Status == RoundStatus.TieBreaker);
	}
}
