using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Database.Repositories;
using BanchoMultiplayerBot.Extensions;
using BanchoMultiplayerBot.Interfaces;
using Serilog;

namespace BanchoMultiplayerBot.Commands;

public class RemoveAdminCommand : IPlayerCommand
{
    public string Command => "removeadmin";

    public List<string>? Aliases => ["deop", "deladmin"];

    public bool AllowGlobal => true;

    public bool Administrator => true;

    public int MinimumArguments => 1;

    public string Usage => BotMessages.Chat.Usage.RemoveAdmin;

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

        // Protection: Bot owner cannot be demoted
        var botOwner = context.Bot.BotConfiguration.BanchoClientConfiguration.Username;
        if (targetUserName.ToIrcNameFormat().Equals(botOwner.ToIrcNameFormat(), StringComparison.OrdinalIgnoreCase) ||
            targetUserName.Equals("AutohostApp", StringComparison.OrdinalIgnoreCase))
        {
            context.Reply(BotMessages.Chat.Admin.CannotDemoteOwner);
            return;
        }

        // Protection: Cannot demote self
        if (targetUserName.ToIrcNameFormat().Equals(context.Message.Sender.ToIrcNameFormat(), StringComparison.OrdinalIgnoreCase))
        {
            context.Reply(BotMessages.Chat.Admin.CannotDemoteSelf);
            return;
        }

        await using var userRepo = new UserRepository();
        var user = await userRepo.FindUserAsync(targetUserName);

        if (user == null || !user.Administrator)
        {
            context.Reply(BotMessages.Chat.Admin.NotAdministrator(targetUserName));
            return;
        }

        user.Administrator = false;
        await userRepo.SaveAsync();

        Log.Information(
            "User '{TargetUser}' demoted from Administrator by '{AdminUser}'",
            user.Name,
            context.Message.Sender);

        context.Reply(BotMessages.Chat.Admin.PlayerDemoted(user.Name, context.Message.Sender));
        context.Bot.NotificationManager.Notify(BotMessages.Discord.AdminDemoted(user.Name, context.Message.Sender));
    }
}

