using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IManualCardMarkingService
{
	Task<ManualCardMarkingResult> Mark(Guid eventId, string cardCode, int number, CancellationToken cancellationToken);
}
