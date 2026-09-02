using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class ManualCardMarkingService(IManualCardMarkingRepository repository, IBingoRoundGameplayService gameplayService) : IManualCardMarkingService
{
	public async Task<ManualCardMarkingResult> Mark(Guid eventId, string cardCode, int number, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		var card = await repository.GetCard(eventId, cardCode, cancellationToken);
		var round = await repository.GetDrawingRound(eventId, cancellationToken);
		if (bingoEvent is null || card is null || round is null) return new ManualCardMarkingResult(false, false, null);
		if (card.Type == CardType.Printed) throw new InvalidOperationException("Cartelas impressas devem ser conferidas pelo QR Code do operador.");
		if (bingoEvent.MarkingMode == CardMarkingMode.Automatic) throw new InvalidOperationException("Este evento usa marcação automática.");
		if (!ContainsNumber(card, number)) throw new ArgumentException("Número inválido para a cartela.");
		var drawn = round.DrawnNumbers.SingleOrDefault(item => item.Number == number) ?? throw new ArgumentException("O número ainda não foi sorteado.");
		if (bingoEvent.MarkingMode == CardMarkingMode.ManualRequired && drawn.Sequence != round.DrawnNumbers.Max(item => item.Sequence)) throw new InvalidOperationException("Na marcação manual obrigatória, somente a pedra atual pode ser marcada.");
		if (await repository.HasMark(round.Id, card.Id, number, cancellationToken)) return new ManualCardMarkingResult(true, false, round.Id);

		repository.AddMark(new CardMark(round.Id, card.Id, number, drawn.Sequence));
		var markedNumbers = (await repository.GetMarkedNumbers(round.Id, card.Id, cancellationToken)).Append(number).ToHashSet();
		var isCardExcluded = await repository.IsExcludedFromActiveStage(round.Id, round.ActiveStage.Id, card.Id, cancellationToken);
		var winnerDetection = gameplayService.DetectManualWinner(round, card, markedNumbers, drawn.Sequence, isCardExcluded);
		if (winnerDetection.HasWinner) repository.AddWinner(winnerDetection.Winner!);
		repository.AddAuditEntry(new AuditEntry(eventId, "Número marcado", $"Cartela {card.PublicCode} marcou a pedra {number} na rodada {round.Name}.", round.Id));
		if (winnerDetection.HasWinner) repository.AddAuditEntry(new AuditEntry(eventId, "Prêmio detectado", "Uma cartela manual atingiu a regra ativa.", round.Id));
		await repository.SaveChanges(cancellationToken);
		return new ManualCardMarkingResult(true, winnerDetection.HasWinner, round.Id);
	}

	private static bool ContainsNumber(BingoCard card, int number) => Enumerable.Range(0, 5).SelectMany(row => Enumerable.Range(0, 5).Select(column => card.Numbers[row, column])).Contains(number);
}
