namespace BingoCompany.Domain.Queries;

public sealed record CompanyEventAwardedCard(Guid WinnerId, string PublicCode, string ParticipantName, string RoundName, string PrizeName);
