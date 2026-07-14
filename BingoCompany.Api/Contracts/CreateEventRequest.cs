using BingoCompany.Domain;

namespace BingoCompany.Api.Contracts;

public sealed record CreateEventRequest(string Name, int CardsPerParticipant = 1, CardMarkingMode MarkingMode = CardMarkingMode.Automatic);
