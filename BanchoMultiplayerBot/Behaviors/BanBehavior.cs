using BanchoMultiplayerBot.Attributes;
using BanchoMultiplayerBot.Behaviors.Data;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Database.Models;
using BanchoMultiplayerBot.Database.Repositories;
using BanchoMultiplayerBot.Extensions;
using BanchoMultiplayerBot.Interfaces;
using BanchoMultiplayerBot.Providers;
using BanchoSharp.Multiplayer;
using Serilog;

namespace BanchoMultiplayerBot.Behaviors
{
    public class BanBehavior(BehaviorEventContext context) : IBehavior, IBehaviorDataConsumer
    {
        private readonly BehaviorDataProvider<BanBehaviorData> _dataProvider = new(context.Lobby);
        private BanBehaviorData Data => _dataProvider.Data;
        public async Task SaveData() => await _dataProvider.SaveData();

        [BanchoEvent(BanchoEventType.PlayerJoined)]
        public async Task OnPlayerJoined(MultiplayerPlayer player)
        {
            var bans = await GetActivePlayerBans(player.Name);

            if (bans.All(x => x.HostBan))
            {
                return;
            }

            // We do this player join record crap since we do not ever really want to use the `!mp ban` command
            // unless we absolutely have to, since there is no "!mp unban" command, so the player will remain banned
            // until the lobby gets recreated. This prevents us from controlling whenever a user should be unbanned or
            // even worse, if a user was wrongfully banned it may not be possible to unban the player. We do still
            // keep a "fail-safe" to `!mp ban` if the player keeps rejoining quickly in a small timespan since it
            // may otherwise be abused to spam the chat with "!mp kick" messages.

            if (!Data.JoinedRecords.TryGetValue(player.Name, out BanBehaviorData.PlayerJoinedRecord? record))
            {
                record = new BanBehaviorData.PlayerJoinedRecord
                {
                    Name = player.Name,
                    Frequency = 0,
                    LastJoinTime = DateTime.UtcNow,
                };

                Data.JoinedRecords.Add(player.Name, record);
            }

            // If it has been more than 15 minutes since the last join, reset the frequency
            // we only care about short-term spam crap
            if ((DateTime.UtcNow - record.LastJoinTime).TotalMinutes > 15)
            {
                Log.Information(BotMessages.Logs.Behaviors.BanResettingJoinFrequency, context.Lobby.LobbyConfigurationId, player.Name);
                record.Frequency = 0;
            }

            record.Frequency++;
            record.LastJoinTime = DateTime.UtcNow;

            var method = record.Frequency > 5 ? "ban" : "kick";

            Log.Information(BotMessages.Logs.Behaviors.BannedUserJoinedRoom,
                player.Name, context.Lobby.LobbyConfigurationId, record.Frequency, method);

            context.SendMessage($"!mp {method} {player.Name.ToIrcNameFormat()}");

            if (record.Frequency > 5)
            {
                context.Lobby.Bot.NotificationManager.Notify(
                    BotMessages.Discord.BannedPlayerSpamAutoBan(context.Lobby.LobbyConfigurationId, context.Channel, player.Name, record.Frequency));
            }
        }

        [BotEvent(BotEventType.CommandExecuted, "Ban")]
        public async Task OnBanCommandExecuted(CommandEventContext commandEventContext)
        {
            await HandleBanCommand(commandEventContext, true);
        }
        
        [BotEvent(BotEventType.CommandExecuted, "PlayerBan")]
        public async Task OnPlayerBanCommandExecuted(CommandEventContext commandEventContext)
        {
            await HandleBanCommand(commandEventContext, false);
        }
        
        [BotEvent(BotEventType.CommandExecuted, "BanMapset")]
        public async Task OnBanMapsetCommandExecuted(CommandEventContext commandEventContext)
        {   
            if (commandEventContext.Arguments.Length == 0)
            {
                context.SendMessage(BotMessages.Chat.Usage.Format(commandEventContext.PlayerCommand.Usage));
                return;
            }

            if (!int.TryParse(commandEventContext.Arguments[0], out int mapSetId))
            {
                context.SendMessage(BotMessages.Chat.Bans.InvalidMapsetId);
                return;
            }
            
            await using var mapBanRepository = new MapBanRepository();
            
            await mapBanRepository.AddAsync(new MapBan
            {
                BeatmapSetId = mapSetId,
            });
            
            await mapBanRepository.SaveAsync();
            
            Log.Information(BotMessages.Logs.Behaviors.MapsetBanned, mapSetId, context.Lobby.LobbyConfigurationId, commandEventContext.Message.Sender);
            context.SendMessage(BotMessages.Chat.Bans.MapsetBanned(mapSetId));
            context.Lobby.Bot.NotificationManager.Notify(
                BotMessages.Discord.MapsetBanned(context.Lobby.LobbyConfigurationId, context.Channel, mapSetId, commandEventContext.Message.Sender));
        }

        private async Task HandleBanCommand(CommandEventContext commandEventContext, bool hostBan)
        {
            if (commandEventContext.Arguments.Length < 2)
            {
                context.SendMessage(BotMessages.Chat.Usage.Format(commandEventContext.PlayerCommand.Usage));
                return;
            }
            
            var playerName = commandEventContext.Arguments[0];
            var user = await GetUserByName(playerName);
            
            if (user == null)
            {
                context.SendMessage(BotMessages.Chat.Bans.UserNotFound);
                return;
            }

            if (!int.TryParse(commandEventContext.Arguments[1], out int lengthDays))
            {
                context.SendMessage(BotMessages.Chat.Bans.InvalidBanLength);
                return;
            }

            var reason = commandEventContext.Arguments.Length >= 3
                ? string.Join(" ", commandEventContext.Arguments.Skip(2))
                : "N/A";
            var expireUtc = DateTime.UtcNow + TimeSpan.FromDays(lengthDays);
            
            await using var banRepository = new PlayerBanRepository();
            
            await banRepository.CreateBan(user, hostBan, reason, expireUtc);
            await banRepository.SaveAsync();
            
            Log.Information(BotMessages.Logs.Behaviors.PlayerBanned,
                context.Lobby.LobbyConfigurationId,
                commandEventContext.Message.Sender,
                user.Name,
                hostBan ? "HostBan" : "PlayerBan",
                lengthDays,
                reason);

            context.SendMessage(BotMessages.Chat.Bans.PlayerBanned(user.Name, hostBan, lengthDays, reason));
            context.Lobby.Bot.NotificationManager.Notify(
                BotMessages.Discord.PlayerBanned(
                    context.Lobby.LobbyConfigurationId,
                    context.Channel,
                    user.Name,
                    commandEventContext.Message.Sender,
                    hostBan,
                    lengthDays,
                    expireUtc,
                    reason));

            if (!hostBan)
            {
                context.SendMessage($"!mp kick {context.GetPlayerIdentifier(playerName)}");
            }
        }

        private async Task<User?> GetUserByName(string inputUserName)
        {
            await using var userRepository = new UserRepository();
    
            var userName = context.MultiplayerLobby.Players.Where(x => x.Name.ToIrcNameFormat().ToLower() == inputUserName.ToIrcNameFormat().ToLower())
                .Select(x => x.Name).FirstOrDefault();
            
            if (userName == null)
            {
                // Just use the username at face value instead.
                userName = inputUserName;
            }
            
            return await userRepository.FindUserAsync(userName);
        }
        
        private static async Task<IEnumerable<PlayerBan>> GetActivePlayerBans(string playerName)
        {
            await using var userRepository = new UserRepository();

            var user = await userRepository.FindUserAsync(playerName);

            return user?.Bans.Where(x => x.Active && (x.Expire == null || x.Expire > DateTime.UtcNow)).ToList() ??
                   Enumerable.Empty<PlayerBan>();
        }
    }
}
