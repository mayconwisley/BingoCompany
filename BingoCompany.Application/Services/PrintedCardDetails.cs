using BingoCompany.Domain.Enums;

namespace BingoCompany.Application.Services;

public sealed record PrintedCardDetails(string PublicCode, string Fingerprint, CardStatus Status, string CompanyName, int[][] Numbers, string QrCodeValue);
