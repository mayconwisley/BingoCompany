using BingoCompany.Application.Services;

namespace BingoCompany.Application.Interfaces;

public interface IPrintedCardRegistrationService
{
	Task<PrintedCardRegistrationResult?> Register(Guid eventId, string cardCode, PrintedCardParticipantRegistration registration, CancellationToken cancellationToken);
}
