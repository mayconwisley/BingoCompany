namespace BingoCompany.Api.Contracts;

public sealed record AuthenticatedSessionResponse(string Name, string CompanyName, string AccountType);
