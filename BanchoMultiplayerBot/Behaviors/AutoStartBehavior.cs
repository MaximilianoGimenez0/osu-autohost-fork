using BanchoMultiplayerBot.Attributes;
using BanchoMultiplayerBot.Bancho.Commands;
using BanchoMultiplayerBot.Behaviors.Config;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Interfaces;
using BanchoMultiplayerBot.Providers;
using Serilog;
using ITimer = BanchoMultiplayerBot.Interfaces.ITimer;

namespace BanchoMultiplayerBot.Behaviors;

public class AutoStartBehavior : IBehavior
{
    private readonly BehaviorEventContext _context;
 
    private readonly BehaviorConfigProvider<AutoStartBehaviorConfig> _configProvider;
    private AutoStartBehaviorConfig Config => _configProvider.Data;
    
    private readonly ITimer? _autoStartTimer;
    private readonly IVote? _startVote;
    
    public AutoStartBehavior(BehaviorEventContext behaviorEventContext)
    {
        _context = behaviorEventContext;
        _configProvider = new BehaviorConfigProvider<AutoStartBehaviorConfig>(_context.Lobby);

        _autoStartTimer = _context.Lobby.TimerProvider?.FindOrCreateTimer("AutoStartTimer");
        _startVote = _context.Lobby.VoteProvider?.FindOrCreateVote("StartVote", BotMessages.Chat.Votes.StartDescription);
    }

    [BotEvent(BotEventType.CommandExecuted, "Start")]
    public async Task OnStartCommandExecuted(CommandEventContext commandEventContext)
    {
        if (commandEventContext.Player == null)
        {
            return;
        }

        // If the player isn't an admin or the host, start a vote
        if (!commandEventContext.User.Administrator && commandEventContext.Player != commandEventContext.Lobby?.MultiplayerLobby?.Host)
        {
            if (_startVote?.PlayerVote(commandEventContext.Player) != true)
            {
                return;
            }
            
            _autoStartTimer?.Stop();
            
            await _context.ExecuteCommandAsync<MatchStartCommand>();

            return;
        }

        // If no seconds argument is provided, start the match immediately
        if (commandEventContext.Arguments.Length == 0)
        {
            _autoStartTimer?.Stop();
            
            await _context.ExecuteCommandAsync<MatchStartCommand>();

            return;
        }

        if (!int.TryParse(commandEventContext.Arguments[0], out int seconds))
        {
            _context.SendMessage(BotMessages.Chat.Usage.Format(commandEventContext.PlayerCommand.Usage));
            return;
        }
        
        StartTimer(TimeSpan.FromSeconds(seconds), true);
    }
    
    [BotEvent(BotEventType.CommandExecuted, "Stop")]
    public void OnStopCommandExecuted(CommandEventContext commandEventContext)
    {
        // Make sure the player is the host or an admin
        if (!commandEventContext.User.Administrator)
        {
            if (commandEventContext.Player != commandEventContext.Lobby?.MultiplayerLobby?.Host)
            {
                return;
            }   
        }
        
        _autoStartTimer?.Stop();
    }
    
    [BanchoEvent(BanchoEventType.AllPlayersReady)]
    public async Task OnAllPlayersReady()
    {
        _autoStartTimer?.Stop();
        
        await _context.ExecuteCommandAsync<MatchStartCommand>();
    }
    
    [BotEvent(BotEventType.BehaviourEvent, "MapManagerNewMap")]
    public void OnNewMap()
    {
        StartTimer(TimeSpan.FromSeconds(Config.NewMapTimer), false);
    }
    
    [BotEvent(BotEventType.TimerElapsed, "AutoStartTimer")]
    public async Task OnAutoStartTimerElapsed()
    {
        if (_context.Lobby.MultiplayerLobby?.Players.Count == 0)
        {
            Log.Warning(BotMessages.Logs.Behaviors.AutoStartIgnoredNoPlayers, _context.Lobby.LobbyConfigurationId);
            return;
        }

        if (_context.MultiplayerLobby.HostIsChangingMap)
        {
            Log.Warning(BotMessages.Logs.Behaviors.AutoStartIgnoredChangingMap, _context.Lobby.LobbyConfigurationId);
            return;
        }
        
        await _context.ExecuteCommandAsync<MatchStartCommand>();
    }
 
    [BotEvent(BotEventType.TimerEarlyWarning, "AutoStartTimer")]
    public void OnAutoStartTimerEarlyWarning()
    {
        _context.SendMessage(BotMessages.Chat.AutoStart.StartingIn10Seconds);
    }

    private void StartTimer(TimeSpan length, bool announceTimer)
    {
        if (length.TotalSeconds <= Config.StartTimerMinimumSeconds || length.TotalSeconds >= Config.StartTimerMaximumSeconds)
        {
            Log.Warning(BotMessages.Logs.Behaviors.AutoStartIgnoredInvalidDuration, _context.Lobby.LobbyConfigurationId, length);
            return;
        }
        
        // Start the timer with an early warning of 10 seconds, if the length is longer than 10 seconds.
        _autoStartTimer?.Start(length, length.TotalSeconds > Config.StartEarlyWarning ? Config.StartEarlyWarning : 0);

        if (announceTimer)
        {
            _context.SendMessage(BotMessages.Chat.AutoStart.QueuedToStart((int)length.TotalSeconds));
        }
    }
    
    private void AbortTimer()
    {
        _autoStartTimer?.Stop();
        _startVote?.Abort();
    }
    
    [BanchoEvent(BanchoEventType.MatchStarted)]
    public void OnMatchStarted() => AbortTimer();
    [BanchoEvent(BanchoEventType.HostChanged)]
    public void OnHostChanged() => AbortTimer();
    [BanchoEvent(BanchoEventType.HostChangingMap)]
    public void OnHostChangingMap() => AbortTimer();
}
