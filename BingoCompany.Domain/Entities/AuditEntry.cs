namespace BingoCompany.Domain.Models;

public sealed class AuditEntry
{
	private AuditEntry() { }
	public AuditEntry(Guid eventId, string action, string details)
	{
		Id = Guid.CreateVersion7();
		EventId = eventId;
		Action = action;
		Details = details;
		OccurredAt = DateTimeOffset.UtcNow;
	}
	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public string Action { get; private set; } = null!;
	public string Details { get; private set; } = null!;
	public DateTimeOffset OccurredAt { get; private set; }
}
