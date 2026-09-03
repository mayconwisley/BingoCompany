namespace BingoCompany.Domain.Entities;

public sealed class CardPurchaseWaitlistEntry
{
	private CardPurchaseWaitlistEntry() { }
	public CardPurchaseWaitlistEntry(Guid eventId, Guid participantAccountId, int requestedQuantity)
	{
		if (requestedQuantity is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(requestedQuantity), "Informe entre 1 e 100 cartelas para a lista de espera.");
		Id = Guid.CreateVersion7();
		EventId = eventId;
		ParticipantAccountId = participantAccountId;
		RequestedQuantity = requestedQuantity;
		CreatedAt = DateTimeOffset.UtcNow;
	}

	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public Guid ParticipantAccountId { get; private set; }
	public int RequestedQuantity { get; private set; }
	public DateTimeOffset CreatedAt { get; private set; }
}
