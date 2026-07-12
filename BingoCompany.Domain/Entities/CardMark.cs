namespace BingoCompany.Domain.Models;

public sealed class CardMark
{
	private CardMark() { }
	public CardMark(Guid roundId, Guid cardId, int number, int drawSequence)
	{
		Id = Guid.CreateVersion7();
		RoundId = roundId;
		CardId = cardId;
		Number = number;
		DrawSequence = drawSequence;
		MarkedAt = DateTimeOffset.UtcNow;
	}
	public Guid Id { get; private set; }
	public Guid RoundId { get; private set; }
	public Guid CardId { get; private set; }
	public int Number { get; private set; }
	public int DrawSequence { get; private set; }
	public DateTimeOffset MarkedAt { get; private set; }
}
