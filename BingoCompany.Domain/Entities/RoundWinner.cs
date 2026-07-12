namespace BingoCompany.Domain.Models;

public sealed class RoundWinner
{
	private RoundWinner() { }
	public RoundWinner(Guid roundId, Guid stageId, Guid cardId, Guid participantId, int drawSequence)
	{
		Id = Guid.CreateVersion7();
		RoundId = roundId;
		StageId = stageId;
		CardId = cardId;
		ParticipantId = participantId;
		DrawSequence = drawSequence;
		DetectedAt = DateTimeOffset.UtcNow;
	}
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
