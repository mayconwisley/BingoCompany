using BingoCompany.Domain;

namespace BingoCompany.Api.Contracts;

public sealed record CreateEventRequest(string Name, CardMarkingMode MarkingMode = CardMarkingMode.Automatic);
