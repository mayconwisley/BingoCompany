using System.ComponentModel.DataAnnotations;

namespace BingoCompany.Api.Contracts;

public sealed record RegisterParticipantAccountRequest(
	[Required, StringLength(120, MinimumLength = 2)] string Name,
	[Required, EmailAddress, StringLength(254)] string Email,
	[Required, StringLength(128, MinimumLength = 12)] string Password);
