using BingoCompany.Domain;

namespace BingoCompany.Api.Contracts;

public sealed record RegisterPrintedCardParticipantRequest(string Name, ParticipantType Type, string? EmployeeRegistration, string? ResponsibleEmployeeName);
