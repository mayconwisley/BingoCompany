using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IPrintedWinnerValidationService
{
	Task<PrintedWinnerValidationResult?> Validate(Guid eventId, Guid roundId, string cardCode, CancellationToken cancellationToken);
}
