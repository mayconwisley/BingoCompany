namespace BingoCompany.Domain.Entities;

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
	public bool HasWinnerPresentationPending => _stages.Any(stage => stage.IsCompleted && !stage.IsWinnerPresentationClosed);
	public void AddStage(PrizeStage stage)
	{
		if (_stages.Any(item => item.Sequence == stage.Sequence)) throw new InvalidOperationException("A sequência das etapas de prêmio não pode se repetir.");
		if (_stages.Any(item => item.Pattern == stage.Pattern)) throw new InvalidOperationException("Cada regra de premiação pode ser usada apenas uma vez por rodada.");

		_stages.Add(stage);
	}
	public void Update(string name, IReadOnlyCollection<PrizeStage> stages)
	{
		if (Status != RoundStatus.Ready) throw new InvalidOperationException("A rodada só pode ser editada antes do início do sorteio.");
		if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Informe o nome da rodada.");
		if (stages.Count == 0) throw new InvalidOperationException("Configure ao menos uma etapa de prêmio.");
		if (stages.Any(stage => stage.RoundId != Id)) throw new InvalidOperationException("As etapas de prêmio devem pertencer à rodada editada.");
		if (stages.GroupBy(stage => stage.Sequence).Any(group => group.Count() > 1)) throw new InvalidOperationException("A sequência das etapas de prêmio não pode se repetir.");
		if (stages.GroupBy(stage => stage.Pattern).Any(group => group.Count() > 1)) throw new InvalidOperationException("Cada regra de premiação pode ser usada apenas uma vez por rodada.");

		Name = name;
		_stages.Clear();
		_stages.AddRange(stages);
	}
	public void FreezeEligibility(IEnumerable<BingoCard> cards)
	{
		if (_eligibleCards.Count != 0) throw new InvalidOperationException("A elegibilidade desta rodada já foi congelada.");
		foreach (var card in cards.Where(x => x.IsEligible)) _eligibleCards.Add(new RoundEligibleCard(Id, card.Id, card.ParticipantId!.Value));
	}
	public void Start(int[] sequence, string hash)
	{
		if (Status != RoundStatus.Ready) throw new InvalidOperationException("Rodada já iniciada.");
		if (_stages.Count == 0) throw new InvalidOperationException("Configure ao menos uma etapa de prêmio antes de iniciar a rodada.");
		if (sequence.Length != 75 || sequence.Distinct().Count() != 75 || sequence.Any(number => number is < 1 or > 75)) throw new InvalidOperationException("A sequência de sorteio deve conter as 75 pedras sem repetição.");

		DrawSequence = sequence;
		SequenceHash = hash;
		Status = RoundStatus.Drawing;
		_stages.OrderBy(x => x.Sequence).First().Activate();
	}
	public DrawnNumber DrawNext()
	{
		if (Status != RoundStatus.Drawing || DrawSequence is null) throw new InvalidOperationException("Rodada não está em sorteio.");
		if (HasWinnerPresentationPending) throw new InvalidOperationException("Encerre a apresentação do vencedor antes de sortear a próxima pedra.");
		var drawnNumbers = _drawnNumbers.Select(item => item.Number).ToHashSet();
		var next = DrawSequence.FirstOrDefault(number => !drawnNumbers.Contains(number) && IsEligibleForActiveStage(number));
		if (next == 0) throw new InvalidOperationException("Não há mais pedras elegíveis para a etapa de prêmio ativa.");

		var drawn = new DrawnNumber(Id, next, _drawnNumbers.Count + 1);
		_drawnNumbers.Add(drawn);
		return drawn;
	}
	public PrizeStage ActiveStage => _stages.Single(x => x.IsActive);
	public void DetectWinner() => Status = RoundStatus.WinnerDetected;
	public void StartTieBreaker()
	{
		if (Status != RoundStatus.WinnerDetected) throw new InvalidOperationException("O desempate só pode iniciar após a detecção de vencedores.");
		Status = RoundStatus.TieBreaker;
	}
	public void FinishStage() { ActiveStage.Complete(); var next = _stages.OrderBy(x => x.Sequence).FirstOrDefault(x => !x.IsCompleted); if (next is null) Status = RoundStatus.Finished; else { next.Activate(); Status = RoundStatus.Drawing; } }
	public void CloseWinnerPresentation()
	{
		var stage = _stages.OrderByDescending(item => item.Sequence).FirstOrDefault(item => item.IsCompleted && !item.IsWinnerPresentationClosed)
			?? throw new InvalidOperationException("Não há apresentação de vencedor aguardando encerramento.");
		stage.CloseWinnerPresentation();
	}
	private bool IsEligibleForActiveStage(int number) => ActiveStage.Pattern switch
	{
		WinningPattern.BColumn => number is >= 1 and <= 15,
		WinningPattern.IColumn => number is >= 16 and <= 30,
		WinningPattern.NColumn => number is >= 31 and <= 45,
		WinningPattern.GColumn => number is >= 46 and <= 60,
		WinningPattern.OColumn => number is >= 61 and <= 75,
		_ => true
	};
}
