namespace BingoCompany.Domain.Entities;

public sealed class CardPurchaseInvitation
{
	private CardPurchaseInvitation() { }
	public CardPurchaseInvitation(Guid eventId, int bonusCards, DateTimeOffset? expiresAt = null)
	{
		if (bonusCards is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(bonusCards), "Informe entre 1 e 100 cartelas de bônus.");
		if (expiresAt.HasValue && expiresAt.Value <= DateTimeOffset.UtcNow) throw new InvalidOperationException("A validade do convite deve estar no futuro.");

		Id = Guid.CreateVersion7();
		EventId = eventId;
		Code = Convert.ToHexString(Guid.NewGuid().ToByteArray())[..12];
		BonusCards = bonusCards;
		ExpiresAt = expiresAt;
		CreatedAt = DateTimeOffset.UtcNow;
	}

	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public string Code { get; private set; } = null!;
	public int BonusCards { get; private set; }
	public Guid? RedeemedByParticipantAccountId { get; private set; }
	public DateTimeOffset? RedeemedAt { get; private set; }
	public DateTimeOffset? ExpiresAt { get; private set; }
	public DateTimeOffset CreatedAt { get; private set; }
	public bool IsAvailableAt(DateTimeOffset instant) => !RedeemedByParticipantAccountId.HasValue && (!ExpiresAt.HasValue || ExpiresAt.Value > instant);
	public void Redeem(Guid participantAccountId)
	{
		if (!IsAvailableAt(DateTimeOffset.UtcNow)) throw new InvalidOperationException("Este convite não está mais disponível.");
		RedeemedByParticipantAccountId = participantAccountId;
		RedeemedAt = DateTimeOffset.UtcNow;
	}
}
