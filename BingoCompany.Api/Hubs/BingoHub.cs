using Microsoft.AspNetCore.SignalR;

namespace BingoCompany.Api.Hubs;

public sealed class BingoHub : Hub
{
	public Task JoinRound(Guid roundId) => Groups.AddToGroupAsync(Context.ConnectionId, $"round:{roundId}");
	public Task JoinEvent(Guid eventId) => Groups.AddToGroupAsync(Context.ConnectionId, $"event:{eventId}");
}
