namespace BingoCompany.Api.Contracts;

public sealed record JoinEventRequest(string Name, ParticipantType Type = ParticipantType.Employee, string? EmployeeRegistration = null, string? ResponsibleEmployeeName = null, int? CardsQuantity = null);
