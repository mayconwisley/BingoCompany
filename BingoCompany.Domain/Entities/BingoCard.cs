namespace BingoCompany.Domain.Models;

public sealed class BingoCard
{
	private BingoCard() { }
	public BingoCard(Guid eventId, Guid? participantId, CardType type, int[,] numbers)
	{
		Id = Guid.CreateVersion7();
		EventId = eventId;
		ParticipantId = participantId;
		Type = type;
		Numbers = numbers;
		PublicCode = Convert.ToHexString(Guid.NewGuid().ToByteArray())[..12];
		Status = participantId.HasValue ? CardStatus.Active : CardStatus.Generated;
		CreatedAt = DateTimeOffset.UtcNow;
	}
	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public Guid? ParticipantId { get; private set; }
	public CardType Type { get; private set; }
	public CardStatus Status { get; private set; }
	public string PublicCode { get; private set; } = null!;
	public int[,] Numbers { get; private set; } = null!;
	public DateTimeOffset CreatedAt { get; private set; }
	public bool IsEligible => Status == CardStatus.Active && ParticipantId.HasValue;
	public void Assign(Guid participantId)
	{
		if (IsEligible)
			throw new InvalidOperationException("Cartela já associada.");
		ParticipantId = participantId;
		Status = CardStatus.Active;
	}
}
