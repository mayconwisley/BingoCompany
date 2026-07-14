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
	public CardMarkingMode MarkingMode { get; private set; }
	public DateTimeOffset CreatedAt { get; private set; }
	public IReadOnlyCollection<BingoRound> Rounds => _rounds;
	public IReadOnlyCollection<BingoCard> Cards => _cards;
	public IReadOnlyCollection<Participant> Participants => _participants;
	public void OpenRegistration()
	{
		if (Status is EventStatus.Draft)
			Status = EventStatus.RegistrationOpen;
		else
			throw new InvalidOperationException("O evento não pode aceitar inscrições agora.");
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
		if (_rounds.Any(round => round.Status != RoundStatus.Finished)) throw new InvalidOperationException("Finalize todas as rodadas antes de encerrar o evento.");
		Status = EventStatus.Finished;
	}
	public void AddRound(BingoRound round) => _rounds.Add(round);
	public void AddParticipant(Participant participant) => _participants.Add(participant);
	public void AddCard(BingoCard card) => _cards.Add(card);
}
