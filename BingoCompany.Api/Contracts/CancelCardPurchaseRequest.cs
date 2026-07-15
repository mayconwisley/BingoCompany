using System.ComponentModel.DataAnnotations;

namespace BingoCompany.Api.Contracts;

public sealed record CancelCardPurchaseRequest([Required, StringLength(500, MinimumLength = 3)] string Reason);
