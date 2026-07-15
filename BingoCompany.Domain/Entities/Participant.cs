namespace BingoCompany.Domain.Entities;

public sealed class Participant
{
	private Participant() { }
	public Participant(Guid eventId, string name, ParticipantType type = ParticipantType.Employee, string? employeeRegistration = null, string? responsibleEmployeeName = null, Guid? participantAccountId = null)
	{
		Id = Guid.CreateVersion7();
		EventId = eventId;
		Name = name.Trim();
		Type = type;
		EmployeeRegistration = employeeRegistration?.Trim();
		ResponsibleEmployeeName = responsibleEmployeeName?.Trim();
		ParticipantAccountId = participantAccountId;
		JoinedAt = DateTimeOffset.UtcNow;
	}
	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public string Name { get; private set; } = null!;
	public ParticipantType Type { get; private set; }
	public string? EmployeeRegistration { get; private set; }
	public string? ResponsibleEmployeeName { get; private set; }
	public Guid? ParticipantAccountId { get; private set; }
	public DateTimeOffset JoinedAt { get; private set; }
}
