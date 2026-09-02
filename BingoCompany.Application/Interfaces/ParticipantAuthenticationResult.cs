namespace BingoCompany.Application.Interfaces;

public sealed record ParticipantAuthenticationResult(Guid AccountId, string Name, string Email);
