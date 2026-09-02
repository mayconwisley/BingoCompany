using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IEventRoundManagementService
{
	Task<RoundConfigurationResult?> Create(Guid eventId, RoundConfigurationRequest request, CancellationToken cancellationToken);
	Task<bool> Update(Guid eventId, Guid roundId, RoundConfigurationRequest request, CancellationToken cancellationToken);
	Task<StartedRoundResult?> Start(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<CancelledRoundResult?> Cancel(Guid eventId, Guid roundId, CancellationToken cancellationToken);
	Task<bool> FinishEvent(Guid eventId, CancellationToken cancellationToken);
}
