using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventCard(string PublicCode, CardType Type, CardStatus Status, string Fingerprint, Guid? ParticipantId);
