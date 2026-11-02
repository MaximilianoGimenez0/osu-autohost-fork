using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Interfaces;

namespace BanchoMultiplayerBot.Commands;

public class TeamsModeCommand : IPlayerCommand
{
    public string Command => "teamsmode";

    public List<string>? Aliases => [];

    public bool AllowGlobal => false;

    public bool Administrator => true;

    public int MinimumArguments => 1;

    public string? Usage => BotMessages.Chat.Usage.TeamsMode;

    // Handled by the behavior
    public Task ExecuteAsync(CommandEventContext context) => Task.CompletedTask;
}
