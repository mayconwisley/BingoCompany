using BingoCompany.Domain.Enums;

namespace BingoCompany.Domain.Queries;

public sealed record PublicAuditStage(string PrizeName, WinningPattern Pattern, bool IsCompleted);
