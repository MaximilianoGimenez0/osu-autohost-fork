using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Database;
using BanchoMultiplayerBot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BanchoMultiplayerBot.Commands;

public class AdminListCommand : IPlayerCommand
{
    public string Command => "adminlist";

    public List<string>? Aliases => ["admins"];

    public bool AllowGlobal => true;

    public bool Administrator => true;

    public int MinimumArguments => 0;

    public string? Usage => null;

    public async Task ExecuteAsync(CommandEventContext context)
    {
        await using var dbContext = new BotDbContext();

        var adminNames = await dbContext.Users
            .Where(x => x.Administrator)
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync();

        context.Reply(BotMessages.Chat.Admin.AdminList(adminNames));
    }
}

