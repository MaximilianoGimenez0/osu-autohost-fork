using BanchoMultiplayerBot.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace BanchoMultiplayerBot.Host.WebApi.Hubs;

[Authorize]
public class LobbyEventHub : Hub
{
    // Messages are sent out from LobbyTrackerService

    public override Task OnConnectedAsync()
    {
        Log.Information(BotMessages.Logs.Host.HubClientConnected, Context.ConnectionId);
        
        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Log.Information(BotMessages.Logs.Host.HubClientDisconnected, Context.ConnectionId);
        
        return base.OnDisconnectedAsync(exception);
    }
}
