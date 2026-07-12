namespace BingoCompany.Domain.Models;

public sealed class BingoRound
{
	private readonly List<PrizeStage> _stages = [];
	private readonly List<DrawnNumber> _drawnNumbers = [];
	private readonly List<RoundEligibleCard> _eligibleCards = [];
	private BingoRound() { }
	public BingoRound(Guid eventId, int sequence, string name)
	{
		Id = Guid.CreateVersion7();
		EventId = eventId;
		Sequence = sequence;
		Name = name;
		Status = RoundStatus.Ready;
	}
	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public int Sequence { get; private set; }
	public string Name { get; private set; } = null!;
	public RoundStatus Status { get; private set; }
	public string? SequenceHash { get; private set; }
	public int[]? DrawSequence { get; private set; }
	public IReadOnlyCollection<PrizeStage> Stages => _stages;
	public IReadOnlyCollection<DrawnNumber> DrawnNumbers => _drawnNumbers;
	public IReadOnlyCollection<RoundEligibleCard> EligibleCards => _eligibleCards;
	public void AddStage(PrizeStage stage) => _stages.Add(stage);
	public void FreezeEligibility(IEnumerable<BingoCard> cards)
	{
		if (_eligibleCards.Count != 0) throw new InvalidOperationException("A elegibilidade desta rodada já foi congelada.");
		foreach (var card in cards.Where(x => x.IsEligible)) _eligibleCards.Add(new RoundEligibleCard(Id, card.Id, card.ParticipantId!.Value));
	}
	public void Start(int[] sequence, string hash) { if (Status != RoundStatus.Ready) throw new InvalidOperationException("Rodada já iniciada."); DrawSequence = sequence; SequenceHash = hash; Status = RoundStatus.Drawing; _stages.OrderBy(x => x.Sequence).First().Activate(); }
	public DrawnNumber DrawNext()
	{ if (Status != RoundStatus.Drawing || DrawSequence is null) throw new InvalidOperationException("Rodada não está em sorteio."); var next = DrawSequence[_drawnNumbers.Count]; var drawn = new DrawnNumber(Id, next, _drawnNumbers.Count + 1); _drawnNumbers.Add(drawn); return drawn; }
	public PrizeStage ActiveStage => _stages.Single(x => x.IsActive);
	public void DetectWinner() => Status = RoundStatus.WinnerDetected;
	public void FinishStage() { ActiveStage.Complete(); var next = _stages.OrderBy(x => x.Sequence).FirstOrDefault(x => !x.IsCompleted); if (next is null) Status = RoundStatus.Finished; else { next.Activate(); Status = RoundStatus.Drawing; } }
}
