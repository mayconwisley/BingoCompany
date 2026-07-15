namespace BingoCompany.Domain.Entities;

public sealed class BingoEvent
{
	private readonly List<BingoRound> _rounds = [];
	private readonly List<BingoCard> _cards = [];
	private readonly List<Participant> _participants = [];
	private BingoEvent() { }
	public BingoEvent(Guid companyId, string name, int cardsPerParticipant = 1, CardMarkingMode markingMode = CardMarkingMode.Automatic)
	{
		Id = Guid.CreateVersion7();
		CompanyId = companyId;
		Name = name;
		CardsPerParticipant = cardsPerParticipant;
		MarkingMode = markingMode;
		PublicCode = Convert.ToHexString(Guid.NewGuid().ToByteArray())[..10];
		CreatedAt = DateTimeOffset.UtcNow;
	}
	public Guid Id { get; private set; }
	public Guid CompanyId { get; private set; }
	public string Name { get; private set; } = null!;
	public string PublicCode { get; private set; } = null!;
	public EventStatus Status { get; private set; } = EventStatus.Draft;
	public int CardsPerParticipant { get; private set; }
	public bool IsCardPurchaseOpen { get; private set; }
	public int? CardPurchaseLimit { get; private set; }
	public string? CardPurchaseCancellationReason { get; private set; }
	public CardMarkingMode MarkingMode { get; private set; }
	public DateTimeOffset CreatedAt { get; private set; }
	public IReadOnlyCollection<BingoRound> Rounds => _rounds;
	public IReadOnlyCollection<BingoCard> Cards => _cards;
	public IReadOnlyCollection<Participant> Participants => _participants;
	public void OpenRegistration(int cardsPerParticipant = 1)
	{
		if (cardsPerParticipant is < 1 or > 100) throw new InvalidOperationException("Informe entre 1 e 100 cartelas por participante.");
		if (Status is not EventStatus.Draft) throw new InvalidOperationException("O evento não pode aceitar inscrições agora.");
		CardsPerParticipant = cardsPerParticipant;
		Status = EventStatus.RegistrationOpen;
	}
	public void OpenCardPurchase(int quantity)
	{
		if (quantity is < 1 or > 10_000) throw new InvalidOperationException("Informe entre 1 e 10000 cartelas para venda.");
		if (CardPurchaseCancellationReason is not null) throw new InvalidOperationException("A venda de cartelas foi cancelada e não pode ser reaberta.");
		if (Status is not EventStatus.Draft) throw new InvalidOperationException("O evento não pode vender cartelas agora.");
		Status = EventStatus.RegistrationOpen;

		IsCardPurchaseOpen = true;
		CardPurchaseLimit = quantity;
	}
	public void UpdateCardPurchaseLimit(int quantity, int soldCards)
	{
		if (!IsCardPurchaseOpen || Status != EventStatus.RegistrationOpen) throw new InvalidOperationException("A venda de cartelas não está aberta para alteração.");
		if (quantity is < 1 or > 10_000) throw new InvalidOperationException("Informe entre 1 e 10000 cartelas para venda.");
		if (quantity < soldCards) throw new InvalidOperationException("A quantidade não pode ser menor que as cartelas já vendidas.");

		CardPurchaseLimit = quantity;
	}
	public void CancelCardPurchase(string reason)
	{
		if (!IsCardPurchaseOpen || Status != EventStatus.RegistrationOpen) throw new InvalidOperationException("A venda de cartelas não está aberta para cancelamento.");
		if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("Informe o motivo do cancelamento.");

		IsCardPurchaseOpen = false;
		CardPurchaseCancellationReason = reason.Trim();
	}
	public void Start()
	{
		if (Status is not EventStatus.RegistrationOpen)
			throw new InvalidOperationException("Abra as inscrições antes de iniciar.");
		Status = EventStatus.Running;
	}
	public void Finish()
	{
		if (Status != EventStatus.Running) throw new InvalidOperationException("O evento não está em andamento.");
		if (_rounds.Any(round => round.Status is not (RoundStatus.Finished or RoundStatus.Cancelled))) throw new InvalidOperationException("Finalize ou cancele todas as rodadas antes de encerrar o evento.");
		Status = EventStatus.Finished;
	}
	public void AddRound(BingoRound round) => _rounds.Add(round);
	public void AddParticipant(Participant participant) => _participants.Add(participant);
	public void AddCard(BingoCard card) => _cards.Add(card);
}
