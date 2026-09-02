using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface ICardLifecycleService
{
	Task<NextCardResult?> GenerateNext(Guid eventId, string cardCode, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<PrintedCardGenerationResult>?> GeneratePrinted(Guid eventId, int quantity, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<PrintedCardDetails>?> GetPrinted(Guid eventId, CancellationToken cancellationToken);
	Task<bool> AssignPrinted(Guid eventId, string cardCode, Guid participantId, CancellationToken cancellationToken);
	Task<bool> ActivatePrinted(Guid eventId, string cardCode, CancellationToken cancellationToken);
}
