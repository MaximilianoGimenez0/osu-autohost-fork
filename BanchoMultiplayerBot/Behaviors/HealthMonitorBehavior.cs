using BanchoMultiplayerBot.Attributes;
using BanchoMultiplayerBot.Behaviors.Data;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Interfaces;
using BanchoMultiplayerBot.Providers;
using Serilog;

namespace BanchoMultiplayerBot.Behaviors;

public class HealthMonitorBehavior(BehaviorEventContext context) : IBehavior, IBehaviorDataConsumer
{
    private readonly BehaviorDataProvider<HealthMonitorBehaviorData> _dataProvider = new(context.Lobby);
    private HealthMonitorBehaviorData Data => _dataProvider.Data;
 
    public async Task SaveData() => await _dataProvider.SaveData();
    
    [BotEvent(BotEventType.Initialize)]
    public void OnInitialize()
    {
        context.TimerProvider.FindOrCreateTimer("HealthMonitorTimer").Start(TimeSpan.FromMinutes(5));
        
        Data.LastEventTime = DateTime.UtcNow;
    }
    
    [BotEvent(BotEventType.TimerElapsed, "HealthMonitorTimer")]
    public void OnHealthMonitorTimerElapsed()
    {
        if ((DateTime.UtcNow - Data.LastEventTime).TotalHours > 2 &&
            (context.Lobby.Health == LobbyHealth.Ok || context.Lobby.Health == LobbyHealth.Idle))
        {
            var channelName = context.Lobby.MultiplayerLobby?.ChannelName ?? "N/A";
            var lobbyName = context.Lobby.MultiplayerLobby?.Name ?? $"Lobby #{context.Lobby.LobbyConfigurationId}";

            Log.Warning(BotMessages.Logs.Behaviors.HealthMonitorLobbyDead,
                context.Lobby.LobbyConfigurationId,
                channelName,
                Data.LastEventTime);
            
            context.Lobby.Health = LobbyHealth.EventTimeoutReached;
            context.Lobby.Bot.NotificationManager.Notify(
                BotMessages.Discord.LobbyEventTimeout(
                    context.Lobby.LobbyConfigurationId,
                    lobbyName,
                    channelName,
                    Data.LastEventTime));
            
            var lobby = context.Lobby;
            
            _ = Task.Run(async () =>
            {
                try
                {
                    await lobby.ConnectAsync();
                }
                catch (Exception e)
                {
                    Log.Error(e, BotMessages.Logs.Behaviors.HealthMonitorRecreateFailed, lobby.LobbyConfigurationId);
                }
            });
            
            return;
        }
        
        // Continuously check if the lobby is still alive.
        context.TimerProvider.FindOrCreateTimer("HealthMonitorTimer").Start(TimeSpan.FromMinutes(5));
    }

    [BanchoEvent(BanchoEventType.PlayerJoined)]
    public void OnPlayerJoined()
    {
        Data.LastEventTime = DateTime.UtcNow;

        if (context.Lobby.Health == LobbyHealth.Idle)
        {
            context.Lobby.Health = LobbyHealth.Ok;
        }
    }
    
    [BanchoEvent(BanchoEventType.PlayerDisconnected)]
    public void OnPlayerDisconnected()
    {
        Data.LastEventTime = DateTime.UtcNow;
        
        if (context.Lobby.Health == LobbyHealth.Ok && context.Lobby.MultiplayerLobby?.Players.Count == 0)
        {
            context.Lobby.Health = LobbyHealth.Idle;
        }
    }
    
    [BanchoEvent(BanchoEventType.MatchFinished)]
    public void OnMatchFinished() => Data.LastEventTime = DateTime.UtcNow;
}
