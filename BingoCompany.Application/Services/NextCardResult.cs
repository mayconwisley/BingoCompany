namespace BingoCompany.Application.Services;

public sealed record NextCardResult(Guid CardId, string PublicCode, int[][] Numbers);
