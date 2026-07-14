namespace BingoCompany.Domain.Entities;

public sealed class PrizeStage
{
	private PrizeStage() { }
	public PrizeStage(Guid roundId, int sequence, string prizeName, WinningPattern pattern, string? prizeImageDataUrl = null)
	{
		Id = Guid.CreateVersion7();
		RoundId = roundId;
		Sequence = sequence;
		PrizeName = prizeName;
		Pattern = pattern;
		PrizeImageDataUrl = prizeImageDataUrl;
	}
	public Guid Id { get; private set; }
	public Guid RoundId { get; private set; }
	public int Sequence { get; private set; }
	public string PrizeName { get; private set; } = null!;
	public WinningPattern Pattern { get; private set; }
	public string? PrizeImageDataUrl { get; private set; }
	public bool IsActive { get; private set; }
	public bool IsCompleted { get; private set; }
	public bool IsWinnerPresentationClosed { get; private set; }
	public void Activate() { IsActive = true; }
	public void Complete() { IsActive = false; IsCompleted = true; }
	public void CloseWinnerPresentation()
	{
		if (!IsCompleted) throw new InvalidOperationException("A apresentação do vencedor só pode ser encerrada após a etapa ser concluída.");
		if (IsWinnerPresentationClosed) throw new InvalidOperationException("A apresentação deste vencedor já foi encerrada.");

		IsWinnerPresentationClosed = true;
	}
}
