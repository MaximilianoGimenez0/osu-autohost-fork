using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Database.Repositories;
using BanchoMultiplayerBot.Extensions;
using BanchoMultiplayerBot.Interfaces;
using Serilog;

namespace BanchoMultiplayerBot.Commands;

public class AddAdminCommand : IPlayerCommand
{
    public string Command => "addadmin";

    public List<string>? Aliases => ["op", "setadmin"];

    public bool AllowGlobal => true;

    public bool Administrator => true;

    public int MinimumArguments => 1;

    public string Usage => BotMessages.Chat.Usage.AddAdmin;

    public async Task ExecuteAsync(CommandEventContext context)
    {
        var inputUserName = string.Join(" ", context.Arguments).Trim();
        if (string.IsNullOrWhiteSpace(inputUserName))
        {
            context.Reply(BotMessages.Chat.Usage.Format(Usage));
            return;
        }

        // Check if player is currently in the lobby to get accurate casing
        var matchedPlayer = context.Lobby?.MultiplayerLobby?.Players
            .FirstOrDefault(x => x.Name.ToIrcNameFormat().Equals(inputUserName.ToIrcNameFormat(), StringComparison.OrdinalIgnoreCase));

        var targetUserName = matchedPlayer?.Name ?? inputUserName;

        await using var userRepo = new UserRepository();
        var user = await userRepo.FindOrCreateUserAsync(targetUserName);

        if (user.Administrator)
        {
            context.Reply(BotMessages.Chat.Admin.AlreadyAdministrator(user.Name));
            return;
        }

        user.Administrator = true;
        await userRepo.SaveAsync();

        Log.Information(
            "User '{TargetUser}' promoted to Administrator by '{AdminUser}'",
            user.Name,
            context.Message.Sender);

        context.Reply(BotMessages.Chat.Admin.PlayerPromoted(user.Name, context.Message.Sender));
        context.Bot.NotificationManager.Notify(BotMessages.Discord.AdminPromoted(user.Name, context.Message.Sender));
    }
}

