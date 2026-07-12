namespace BingoCompany.Domain.Models;

public sealed class Participant
{
	private Participant() { }
	public Participant(Guid eventId, string name, string? responsibleEmployeeName = null)
	{
		Id = Guid.CreateVersion7();
		EventId = eventId;
		Name = name.Trim();
		ResponsibleEmployeeName = responsibleEmployeeName?.Trim();
		JoinedAt = DateTimeOffset.UtcNow;
	}
	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public string Name { get; private set; } = null!;
	public string? ResponsibleEmployeeName { get; private set; }
	public DateTimeOffset JoinedAt { get; private set; }
}
