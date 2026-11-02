using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Interfaces;

namespace BanchoMultiplayerBot.Commands
{
    public class HelpCommand : IPlayerCommand
    {
        public string Command => "help";

        public List<string>? Aliases => [ "commands", "info" ];

        public bool AllowGlobal => true;

        public bool Administrator => false;

        public int MinimumArguments => 0;

        public string? Usage => null;

        public Task ExecuteAsync(CommandEventContext message)
        {
            message.Reply(BotMessages.Chat.Commands.HelpResponse);
            
            return Task.CompletedTask;
        }
    }
}
