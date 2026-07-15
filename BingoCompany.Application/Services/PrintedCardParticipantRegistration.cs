using BingoCompany.Domain;

namespace BingoCompany.Application.Services;

public sealed record PrintedCardParticipantRegistration(string Name, ParticipantType Type, string? EmployeeRegistration, string? ResponsibleEmployeeName);
