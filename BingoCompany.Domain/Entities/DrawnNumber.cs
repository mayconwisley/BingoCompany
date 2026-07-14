namespace BingoCompany.Domain.Entities;

public sealed class DrawnNumber
{
	private DrawnNumber() { }
	public DrawnNumber(Guid roundId, int number, int sequence)
	{
		Id = Guid.CreateVersion7();
		RoundId = roundId;
		Number = number;
		Sequence = sequence;
		DrawnAt = DateTimeOffset.UtcNow;
	}
	public Guid Id { get; private set; }
	public Guid RoundId { get; private set; }
	public int Number { get; private set; }
	public int Sequence { get; private set; }
	public DateTimeOffset DrawnAt { get; private set; }
}
