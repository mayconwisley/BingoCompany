namespace BingoCompany.Application.Interfaces;

public sealed record CompanyAuthenticationResult(Guid UserId, Guid CompanyId, string UserName, string Email, string CompanyName);
