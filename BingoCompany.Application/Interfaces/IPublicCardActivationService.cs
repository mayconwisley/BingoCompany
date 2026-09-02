using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IPublicCardActivationService
{
	Task<DigitalCardActivationResult> Activate(string publicCode, string cardCode, Guid? participantAccountId, CancellationToken cancellationToken);
}
