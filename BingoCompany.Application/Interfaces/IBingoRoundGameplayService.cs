using BingoCompany.Application.Services;
using BingoCompany.Domain.Entities;

namespace BingoCompany.Application.Interfaces;

public interface IBingoRoundGameplayService
{
	RoundDrawResult Draw(BingoRound round, IReadOnlyCollection<BingoCard> eligibleCards, IReadOnlyCollection<CardMark> marks, CardMarkingMode markingMode);
	RoundWinnerDetection DetectManualWinner(BingoRound round, BingoCard card, IReadOnlySet<int> markedNumbers, int drawSequence);
	WinnerRevealResult RevealWinner(BingoRound round, IReadOnlyCollection<RoundWinner> candidates, DateTimeOffset revealedAt);
}
