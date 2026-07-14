namespace BingoCompany.Domain.Entities;

public sealed class RoundEligibleCard
{
	private RoundEligibleCard() { }
	public RoundEligibleCard(Guid roundId, Guid cardId, Guid participantId)
	{
		Id = Guid.CreateVersion7();
		RoundId = roundId;
		CardId = cardId;
		ParticipantId = participantId;
		IncludedAt = DateTimeOffset.UtcNow;
	}
	public Guid Id { get; private set; }
	public Guid RoundId { get; private set; }
	public Guid CardId { get; private set; }
	public Guid ParticipantId { get; private set; }
	public DateTimeOffset IncludedAt { get; private set; }
}
