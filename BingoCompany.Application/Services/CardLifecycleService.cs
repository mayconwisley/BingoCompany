using BingoCompany.Application.Interfaces;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Domain.Enums;
using BingoCompany.Domain.Repositories;

namespace BingoCompany.Application.Services;

public sealed class CardLifecycleService(ICardLifecycleRepository repository) : ICardLifecycleService
{
	public async Task<NextCardResult?> GenerateNext(Guid eventId, string cardCode, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		var card = await repository.GetCard(eventId, cardCode, cancellationToken);
		if (bingoEvent is null || card is null) return null;
		if (bingoEvent.Status == EventStatus.Finished) throw new InvalidOperationException("O evento encerrado não pode ser alterado.");
		if (card.ReplacementCardId.HasValue) throw new InvalidOperationException("Uma nova cartela já foi gerada a partir desta cartela.");
		var previousRound = await repository.GetLatestCompletedRound(eventId, cancellationToken) ?? throw new InvalidOperationException("A cartela poderá ser renovada após o encerramento de uma rodada.");
		if (await repository.HasStartedRoundAfter(eventId, previousRound.Sequence, cancellationToken)) throw new InvalidOperationException("A próxima rodada já foi iniciada.");
		if (!await repository.WasEligible(previousRound.Id, card.Id, cancellationToken)) throw new InvalidOperationException("Esta cartela não participou da última rodada concluída.");
		var nextCard = new BingoCard(eventId, card.ParticipantId, CardType.Digital, new Bingo75CardGenerator().Generate());
		card.ReplaceWith(nextCard);
		repository.AddCard(nextCard);
		await repository.SaveChanges(cancellationToken);
		return new NextCardResult(nextCard.Id, nextCard.PublicCode, ToRows(nextCard.Numbers));
	}

	public async Task<IReadOnlyCollection<PrintedCardGenerationResult>?> GeneratePrinted(Guid eventId, int quantity, CancellationToken cancellationToken)
	{
		var bingoEvent = await repository.GetEvent(eventId, cancellationToken);
		if (bingoEvent is null) return null;
		if (bingoEvent.Status == EventStatus.Finished) throw new InvalidOperationException("O evento encerrado não pode ser alterado.");
		if (quantity is < 1 or > 1000) throw new ArgumentOutOfRangeException(nameof(quantity), "Informe entre 1 e 1000 cartelas.");
		var cards = Enumerable.Range(0, quantity).Select(_ => new BingoCard(eventId, null, CardType.Printed, new Bingo75CardGenerator().Generate())).ToArray();
		repository.AddCards(cards);
		repository.AddAuditEntry(new AuditEntry(eventId, "Cartelas impressas geradas", $"Lote com {cards.Length} cartelas impressas gerado."));
		await repository.SaveChanges(cancellationToken);
		return cards.Select(card => new PrintedCardGenerationResult(card.PublicCode, card.Fingerprint)).ToArray();
	}

	public async Task<IReadOnlyCollection<PrintedCardDetails>?> GetPrinted(Guid eventId, CancellationToken cancellationToken)
	{
		var companyName = await repository.GetCompanyName(eventId, cancellationToken);
		if (companyName is null) return null;
		var cards = await repository.GetPrintedCards(eventId, cancellationToken);
		return cards.Select(card => new PrintedCardDetails(card.PublicCode, card.Fingerprint, card.Status, companyName, ToRows(card.Numbers), $"BINGO:{eventId:N}:{card.PublicCode}:{card.Fingerprint}")).ToArray();
	}

	public async Task<bool> AssignPrinted(Guid eventId, string cardCode, Guid participantId, CancellationToken cancellationToken)
	{
		var card = await repository.GetCard(eventId, cardCode, cancellationToken);
		var participant = await repository.GetParticipant(eventId, participantId, cancellationToken);
		if (card is null || participant is null) return false;
		if (card.Type != CardType.Printed) throw new InvalidOperationException("Apenas cartelas impressas podem ser associadas por esta operação.");
		card.Assign(participant.Id);
		repository.AddAuditEntry(new AuditEntry(eventId, "Cartela associada", $"Cartela {card.PublicCode} associada a participante."));
		await repository.SaveChanges(cancellationToken);
		return true;
	}

	public async Task<bool> ActivatePrinted(Guid eventId, string cardCode, CancellationToken cancellationToken)
	{
		var card = await repository.GetCard(eventId, cardCode, cancellationToken);
		if (card is null) return false;
		if (card.Type != CardType.Printed) throw new InvalidOperationException("Apenas cartelas impressas podem ser ativadas por esta operação.");
		card.Activate();
		repository.AddAuditEntry(new AuditEntry(eventId, "Cartela ativada", $"Cartela {card.PublicCode} ativada."));
		await repository.SaveChanges(cancellationToken);
		return true;
	}

	private static int[][] ToRows(int[,] card) => Enumerable.Range(0, 5).Select(row => Enumerable.Range(0, 5).Select(column => card[row, column]).ToArray()).ToArray();
}
