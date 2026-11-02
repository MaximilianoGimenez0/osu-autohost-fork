using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Interfaces;

namespace BanchoMultiplayerBot.Commands;

public class RestartCommand : IPlayerCommand
{
    public string Command => "restart";

    public List<string>? Aliases => null;

    public bool AllowGlobal => true;

    public bool Administrator => true;

    public int MinimumArguments => 1;

    public string Usage => BotMessages.Chat.Usage.Restart;

    private static bool _isRestarting;

    public Task ExecuteAsync(CommandEventContext message)
    {
        // Race condition can probably appear here, but... no
        if (_isRestarting)
        {
            message.Reply(BotMessages.Chat.Commands.RestartInProgress);
            return Task.CompletedTask;
        }

        if (!int.TryParse(message.Arguments[0], out var lobbyId))
        {
            message.Reply(BotMessages.Chat.Usage.Format(Usage));
            return Task.CompletedTask;
        }

        _isRestarting = true;
        
        message.Reply(BotMessages.Chat.Commands.RestartingLobby);

        _ = Task.Run(async () =>
        {
            var lobby = message.Bot.Lobbies.FirstOrDefault(lobby => lobby.LobbyConfigurationId == lobbyId);

            if (lobby == null)
            {
                _isRestarting = false;
                return;
            }
            
            await lobby.ConnectAsync();

            var attempts = 0;
            while (lobby.Health is not (LobbyHealth.Ok or LobbyHealth.Idle))
            {
                if (attempts++ > 10)
                {
                    break;
                }

                await Task.Delay(1000);
            }
            
            _isRestarting = false;
        });

        return Task.CompletedTask;
    }
}
