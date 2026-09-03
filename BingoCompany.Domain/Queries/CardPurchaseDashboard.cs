namespace BingoCompany.Domain.Queries;

public sealed record CardPurchaseDashboard(int Total, int Reserved, int Activated, int EligibleForNextRound, int AwaitingActivation, int WaitlistEntries);
