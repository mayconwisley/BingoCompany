namespace BingoCompany.Api.Contracts;

public sealed record JoinEventRequest(string? Name = null, ParticipantType? Type = null, string? EmployeeRegistration = null, string? ResponsibleEmployeeName = null, int? CardsQuantity = null);
