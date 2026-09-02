using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IRoundGameplayCoordinator
{
	Task<RoundDrawOperationResult?> Draw(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<WinnerRevealOperationResult> Reveal(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<PrizeStageOperationResult?> MarkDelivered(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<PrizeStageOperationResult?> MarkDeclined(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<PrizeStageOperationResult?> ClosePresentation(Guid eventId, Guid roundId, CancellationToken cancellationToken);
}
