using BingoCompany.Domain.Enums;

namespace BingoCompany.Application.Services;

public sealed record EventParticipantRegistrationRequest(
	string? Name,
	ParticipantType? Type,
	string? EmployeeRegistration,
	string? ResponsibleEmployeeName,
	int? CardsQuantity,
	string? InvitationCode = null);
