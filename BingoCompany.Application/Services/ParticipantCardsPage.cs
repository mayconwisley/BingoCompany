using BingoCompany.Domain.Queries;

namespace BingoCompany.Application.Services;

public sealed record ParticipantCardsPage(IReadOnlyCollection<ParticipantCardSummary> ActiveCards, IReadOnlyCollection<ParticipantCardSummary> HistoryItems, int Page, int PageSize, int TotalItems, int TotalPages);
