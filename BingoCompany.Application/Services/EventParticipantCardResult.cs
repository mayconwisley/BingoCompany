using BingoCompany.Domain.Enums;

namespace BingoCompany.Application.Services;

public sealed record EventParticipantCardResult(Guid CardId, string PublicCode, int[][] Numbers, CardStatus Status);
