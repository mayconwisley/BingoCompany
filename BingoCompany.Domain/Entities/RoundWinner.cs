namespace BingoCompany.Domain.Entities;

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
	public DateTimeOffset? PrizeDeliveredAt { get; private set; }
	public DateTimeOffset? PrizeDeclinedAt { get; private set; }
	public void AssignTieBreaker(int number) => TieBreakerNumber = number;
	public void Confirm(DateTimeOffset now) { IsWinner = true; RevealedAt = now; }
	public void MarkPrizeDelivered(DateTimeOffset now)
	{
		if (!IsWinner || !RevealedAt.HasValue) throw new InvalidOperationException("O prêmio só pode ser entregue ao vencedor revelado.");
		if (PrizeDeliveredAt.HasValue || PrizeDeclinedAt.HasValue) throw new InvalidOperationException("A situação de entrega deste prêmio já foi registrada.");

		PrizeDeliveredAt = now;
	}
	public void MarkPrizeDeclined(DateTimeOffset now)
	{
		if (!IsWinner || !RevealedAt.HasValue) throw new InvalidOperationException("A ausência do vencedor só pode ser registrada após a revelação.");
		if (PrizeDeliveredAt.HasValue || PrizeDeclinedAt.HasValue) throw new InvalidOperationException("A situação de entrega deste prêmio já foi registrada.");

		PrizeDeclinedAt = now;
	}
}
