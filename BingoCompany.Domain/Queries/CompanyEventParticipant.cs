using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventParticipant(Guid Id, string Name, ParticipantType Type);
