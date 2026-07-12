namespace BingoCompany.Domain.Models;

public sealed class PrizeStage
{
	private PrizeStage() { }
	public PrizeStage(Guid roundId, int sequence, string prizeName, WinningPattern pattern)
	{
		Id = Guid.CreateVersion7();
		RoundId = roundId;
		Sequence = sequence;
		PrizeName = prizeName;
		Pattern = pattern;
	}
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
