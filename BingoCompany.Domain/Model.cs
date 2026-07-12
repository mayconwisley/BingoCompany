namespace BingoCompany.Domain;

public enum EventStatus { Draft, RegistrationOpen, Running, Finished }
public enum RoundStatus { Ready, Drawing, WinnerDetected, Finished }
public enum CardType { Digital, Printed }
public enum CardStatus { Generated, Printed, Active, Cancelled }
public enum WinningPattern { HorizontalLine, TwoHorizontalLines, FourCorners, FullCard }
public enum CardMarkingMode { Automatic, ManualRequired, AssistedManual }

public sealed class BingoEvent
{
    private readonly List<BingoRound> _rounds = [];
    private readonly List<BingoCard> _cards = [];
    private readonly List<Participant> _participants = [];
    private BingoEvent() { }
    public BingoEvent(string name, int cardsPerParticipant = 1, CardMarkingMode markingMode = CardMarkingMode.Automatic)
    {
        Id = Guid.CreateVersion7(); Name = name; CardsPerParticipant = cardsPerParticipant; MarkingMode = markingMode;
        PublicCode = Convert.ToHexString(Guid.NewGuid().ToByteArray())[..10]; CreatedAt = DateTimeOffset.UtcNow;
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string PublicCode { get; private set; } = null!;
    public EventStatus Status { get; private set; } = EventStatus.Draft;
    public int CardsPerParticipant { get; private set; }
    public CardMarkingMode MarkingMode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<BingoRound> Rounds => _rounds;
    public IReadOnlyCollection<BingoCard> Cards => _cards;
    public IReadOnlyCollection<Participant> Participants => _participants;
    public void OpenRegistration() { if (Status is EventStatus.Draft) Status = EventStatus.RegistrationOpen; else throw new InvalidOperationException("O evento não pode aceitar inscrições agora."); }
    public void Start() { if (Status is not EventStatus.RegistrationOpen) throw new InvalidOperationException("Abra as inscrições antes de iniciar."); Status = EventStatus.Running; }
    public void AddRound(BingoRound round) => _rounds.Add(round);
    public void AddParticipant(Participant participant) => _participants.Add(participant);
    public void AddCard(BingoCard card) => _cards.Add(card);
}

public sealed class Participant
{
    private Participant() { }
    public Participant(Guid eventId, string name, string? responsibleEmployeeName = null)
    { Id = Guid.CreateVersion7(); EventId = eventId; Name = name.Trim(); ResponsibleEmployeeName = responsibleEmployeeName?.Trim(); JoinedAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? ResponsibleEmployeeName { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
}

public sealed class BingoCard
{
    private BingoCard() { }
    public BingoCard(Guid eventId, Guid? participantId, CardType type, int[,] numbers)
    { Id = Guid.CreateVersion7(); EventId = eventId; ParticipantId = participantId; Type = type; Numbers = numbers; PublicCode = Convert.ToHexString(Guid.NewGuid().ToByteArray())[..12]; Status = participantId.HasValue ? CardStatus.Active : CardStatus.Generated; CreatedAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid? ParticipantId { get; private set; }
    public CardType Type { get; private set; }
    public CardStatus Status { get; private set; }
    public string PublicCode { get; private set; } = null!;
    public int[,] Numbers { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsEligible => Status == CardStatus.Active && ParticipantId.HasValue;
    public void Assign(Guid participantId) { if (IsEligible) throw new InvalidOperationException("Cartela já associada."); ParticipantId = participantId; Status = CardStatus.Active; }
}

public sealed class BingoRound
{
    private readonly List<PrizeStage> _stages = [];
    private readonly List<DrawnNumber> _drawnNumbers = [];
    private readonly List<RoundEligibleCard> _eligibleCards = [];
    private BingoRound() { }
    public BingoRound(Guid eventId, int sequence, string name)
    { Id = Guid.CreateVersion7(); EventId = eventId; Sequence = sequence; Name = name; Status = RoundStatus.Ready; }
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

public sealed class RoundEligibleCard
{
    private RoundEligibleCard() { }
    public RoundEligibleCard(Guid roundId, Guid cardId, Guid participantId) { Id = Guid.CreateVersion7(); RoundId = roundId; CardId = cardId; ParticipantId = participantId; IncludedAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid RoundId { get; private set; }
    public Guid CardId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public DateTimeOffset IncludedAt { get; private set; }
}

public sealed class CardMark
{
    private CardMark() { }
    public CardMark(Guid roundId, Guid cardId, int number, int drawSequence) { Id = Guid.CreateVersion7(); RoundId = roundId; CardId = cardId; Number = number; DrawSequence = drawSequence; MarkedAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid RoundId { get; private set; }
    public Guid CardId { get; private set; }
    public int Number { get; private set; }
    public int DrawSequence { get; private set; }
    public DateTimeOffset MarkedAt { get; private set; }
}

public sealed class RoundWinner
{
    private RoundWinner() { }
    public RoundWinner(Guid roundId, Guid stageId, Guid cardId, Guid participantId, int drawSequence) { Id = Guid.CreateVersion7(); RoundId = roundId; StageId = stageId; CardId = cardId; ParticipantId = participantId; DrawSequence = drawSequence; DetectedAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid RoundId { get; private set; }
    public Guid StageId { get; private set; }
    public Guid CardId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public int DrawSequence { get; private set; }
    public int? TieBreakerNumber { get; private set; }
    public bool IsWinner { get; private set; }
    public DateTimeOffset DetectedAt { get; private set; }
    public DateTimeOffset? RevealedAt { get; private set; }
    public void AssignTieBreaker(int number) => TieBreakerNumber = number;
    public void Confirm(DateTimeOffset now) { IsWinner = true; RevealedAt = now; }
}

public sealed class PrizeStage
{
    private PrizeStage() { }
    public PrizeStage(Guid roundId, int sequence, string prizeName, WinningPattern pattern) { Id = Guid.CreateVersion7(); RoundId = roundId; Sequence = sequence; PrizeName = prizeName; Pattern = pattern; }
    public Guid Id { get; private set; }
    public Guid RoundId { get; private set; }
    public int Sequence { get; private set; }
    public string PrizeName { get; private set; } = null!;
    public WinningPattern Pattern { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsCompleted { get; private set; }
    public void Activate() { IsActive = true; }
    public void Complete() { IsActive = false; IsCompleted = true; }
}

public sealed class DrawnNumber
{
    private DrawnNumber() { }
    public DrawnNumber(Guid roundId, int number, int sequence) { Id = Guid.CreateVersion7(); RoundId = roundId; Number = number; Sequence = sequence; DrawnAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid RoundId { get; private set; }
    public int Number { get; private set; }
    public int Sequence { get; private set; }
    public DateTimeOffset DrawnAt { get; private set; }
}
