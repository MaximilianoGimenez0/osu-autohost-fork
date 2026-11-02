using BanchoMultiplayerBot.Attributes;
using BanchoMultiplayerBot.Bancho.Commands;
using BanchoMultiplayerBot.Behaviors.Config;
using BanchoMultiplayerBot.Behaviors.Data;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Extensions;
using BanchoMultiplayerBot.Interfaces;
using BanchoMultiplayerBot.Osu;
using BanchoMultiplayerBot.Providers;
using BanchoMultiplayerBot.Utilities;
using BanchoSharp.Multiplayer;
using osu.NET;
using osu.NET.Enums;
using osu.NET.Models.Beatmaps;
using Serilog;

namespace BanchoMultiplayerBot.Behaviors
{
    public class MapManagerBehavior(BehaviorEventContext context) : IBehavior, IBehaviorDataConsumer
    {
        private readonly BehaviorDataProvider<MapManagerBehaviorData> _dataProvider = new(context.Lobby);
        private readonly BehaviorConfigProvider<MapManagerBehaviorConfig> _configProvider = new(context.Lobby);

        private MapManagerBehaviorData Data => _dataProvider.Data;
        private MapManagerBehaviorConfig Config => _configProvider.Data;
        
        public async Task SaveData() => await _dataProvider.SaveData();

        [BanchoEvent(BanchoEventType.MatchStarted)]
        public async Task OnMatchStarted()
        {
            Data.MatchStartTime = DateTime.UtcNow;
            
            await ValidatePlayingMap();
        }
            
        [BanchoEvent(BanchoEventType.MatchFinished)]
        public async Task OnMatchFinished()
        {
            var lobbyConfig = await context.Lobby.GetLobbyConfiguration();
            var channelId = context.Lobby.BanchoConnection.ChannelHandler.GetChannelRuntimeId(context.MultiplayerLobby.ChannelName) ?? 0;
            
            foreach (var player in Data.MatchFinishMessageSubscribers)
            {
                context.Lobby.BanchoConnection.MessageHandler.SendMessage(
                    player,
                    BotMessages.Chat.MapManager.MatchFinishedNotification(channelId, lobbyConfig.Name));
            }

            Data.MatchFinishMessageSubscribers.Clear();
        }

        [BanchoEvent(BanchoEventType.HostChanged)]
        public void OnHostChanged()
        {
            // psst, this allows the violation count to be bypassed by passing
            // the host to someone else, after which the bot will give back the host to the original host.
            Data.HostViolationCount = 0;
        }

        [BanchoEvent(BanchoEventType.MapChanged)]
        public async Task OnMapChanged(BeatmapShell beatmapShell)
        {
            Data.CurrentMapId = beatmapShell.Id;
            
            // If the beatmap is the same as the last one applied, we don't need to do anything.
            // This is to prevent the bot from applying the same beatmap multiple times.
            if (beatmapShell.Id == Data.LastBotAppliedBeatmapId)
            {
                return;
            }

            Data.LastPlayerAppliedBeatmapId = beatmapShell.Id;
            
            try
            {
                var beatmapInfoResult = await context.UsingApiClient(async (apiClient) => await apiClient.GetBeatmapAsync(beatmapShell.Id));
                var beatmapAttributesResult = await context.UsingApiClient(async (apiClient) => await apiClient.GetDifficultyAttributesAsync(beatmapShell.Id));
                
                if (beatmapInfoResult.IsFailure)
                {
                    if (beatmapInfoResult.Error.Type == ApiErrorType.BeatmapNotFound)
                    {
                        Log.Information(BotMessages.Logs.Behaviors.MapManagerBeatmapNotFound,
                            beatmapShell.Id, context.Lobby.LobbyConfigurationId, Data.BeatmapFallbackId);
                
                        await ApplyBeatmap(Data.BeatmapFallbackId);
                
                        context.SendMessage(BotMessages.Chat.MapManager.BeatmapNotSubmitted);

                        return;
                    }

                    // Not sure what we want to do in this scenario, we'll just have to trust the 
                    // players pick valid maps until the API gets into a better state.
                    
                    Log.Error(BotMessages.Logs.Behaviors.MapManagerGetBeatmapFailed,
                        beatmapShell.Id, context.Lobby.LobbyConfigurationId, beatmapInfoResult.Error);
                    
                    context.SendMessage(BotMessages.Chat.MapManager.OsuApiBeatmapError);
                    
                    return;
                }
                
                if (beatmapAttributesResult.IsFailure)
                {
                    Log.Error(BotMessages.Logs.Behaviors.MapManagerGetAttributesFailed,
                        beatmapShell.Id, context.Lobby.LobbyConfigurationId, beatmapAttributesResult.Error);
                    
                    context.SendMessage(BotMessages.Chat.MapManager.OsuApiBeatmapError);

                    return;
                }
                
                var beatmapInfo =  beatmapInfoResult.Value!;
                var beatmapAttributes = beatmapAttributesResult.Value!;
                var lobbyConfig = await context.Lobby.GetLobbyConfiguration();
                var mapValidator = new MapValidator(context.Lobby, lobbyConfig, Config);
                var mapValidationResult = await mapValidator.ValidateBeatmap(beatmapAttributes, beatmapInfo);

                // Special case for double-time, we want to check if
                // the map was rejected for star rating
                if (mapValidationResult == MapValidator.MapStatus.StarRating &&
                    Config.MinimumStarRating >= Math.Round(beatmapAttributes.DifficultyRating, 2) &&
                    Config.AllowDoubleTime)
                {
                    var doubleTimeMapAttributesResult = await context.UsingApiClient(async (apiClient) => await apiClient.GetDifficultyAttributesAsync(beatmapShell.Id, Ruleset.Osu, ["DT"]));
                 
                    // Check if map is OK with double time
                    if (doubleTimeMapAttributesResult.IsSuccess &&
                        await mapValidator.ValidateBeatmap(doubleTimeMapAttributesResult.Value!, null) == MapValidator.MapStatus.Ok)
                    {
                        // Check room settings to make sure double time is enabled
                        await context.ExecuteCommandAsync<RoomSettingsUpdateCommand>();

                        if ((context.MultiplayerLobby.Mods & Mods.DoubleTime) != 0)
                        {
                            await EnforceBeatmapRegulations(beatmapInfo, doubleTimeMapAttributesResult.Value!, MapValidator.MapStatus.Ok, (int)Mods.DoubleTime);

                            return;
                        }
                    }
                }

                await EnforceBeatmapRegulations(beatmapInfo, beatmapAttributes, mapValidationResult);
            }
            catch (HttpRequestException e)
            {
                Log.Error(e, BotMessages.Logs.Behaviors.MapManagerApiTimeout, beatmapShell.Id, context.Lobby.LobbyConfigurationId);
                
                context.SendMessage(BotMessages.Chat.MapManager.OsuApiTimeout);
            }
            catch (Exception e)
            {
                Log.Error(e, BotMessages.Logs.Behaviors.MapManagerUnexpectedException, beatmapShell.Id, context.Lobby.LobbyConfigurationId);
                
                context.SendMessage(BotMessages.Chat.MapManager.InternalBeatmapError);
            }
        }

        private async Task EnforceBeatmapRegulations(BeatmapExtended beatmapModel, DifficultyAttributes difficultyAttributes, MapValidator.MapStatus status, int mods = 0)
        {
            var lobbyConfig = await context.Lobby.GetLobbyConfiguration();
            var beatmapSet = beatmapModel.Set;

            Log.Verbose(BotMessages.Logs.Behaviors.MapManagerEnforcingRegulations,
                beatmapModel.Id, context.Lobby.LobbyConfigurationId, status);

            if (status == MapValidator.MapStatus.Ok)
            {
                Data.BeatmapInfo = new BeatmapInfo
                {
                    Id = beatmapModel.Id,
                    SetId = beatmapModel.SetId,
                    Name = beatmapSet?.Title ?? string.Empty,
                    Artist = beatmapSet?.Artist ?? string.Empty,
                    Length = beatmapModel.TotalLength,
                    DrainLength = beatmapModel.HitLength,
                    StarRating = difficultyAttributes.DifficultyRating
                };

                Data.BeatmapFallbackId = Data.BeatmapInfo.Id;
                
                // By "setting" the map our self directly after the host picked it, 
                // it will automatically be set to the newest version, even if the host's one is outdated.
                await ApplyBeatmap(Data.BeatmapInfo.Id);
                await AnnounceBeatmap(beatmapModel, difficultyAttributes, mods);

                // Fire off any "new map" events
                await context.Lobby.BehaviorEventProcessor!.OnBehaviorEvent("MapManagerNewMap");
                
                return;
            }

            await ApplyBeatmap(Data.BeatmapFallbackId);

            switch (status)
            {
                case MapValidator.MapStatus.Length:
                    context.SendMessage(beatmapModel.TotalLength.TotalSeconds >= Config.MaximumMapLength
                        ? BotMessages.Chat.MapManager.BeatmapTooLong(TimeSpan.FromSeconds(Config.MaximumMapLength))
                        : BotMessages.Chat.MapManager.BeatmapTooShort(TimeSpan.FromSeconds(Config.MinimumMapLength)));
                    
                    break;
                case MapValidator.MapStatus.StarRating:
                    context.SendMessage(difficultyAttributes.DifficultyRating >= Config.MaximumStarRating
                        ? BotMessages.Chat.MapManager.StarRatingTooHigh(difficultyAttributes.DifficultyRating, Config.MaximumStarRating)
                        : BotMessages.Chat.MapManager.StarRatingTooLow(Config.MinimumStarRating, difficultyAttributes.DifficultyRating));
                    break;
                case MapValidator.MapStatus.GameMode:
                    var modeName = lobbyConfig.Mode switch
                    {
                        GameMode.osu => "osu!std",
                        GameMode.osuCatch => "osu!catch",
                        GameMode.osuMania => "osu!mania",
                        GameMode.osuTaiko => "osu!taiko",
                        _ => "Error"
                    };
    
                    context.SendMessage(BotMessages.Chat.MapManager.InvalidGameMode(modeName));
                    break;
                case MapValidator.MapStatus.Banned:
                    context.SendMessage(BotMessages.Chat.MapManager.BeatmapBanned(beatmapSet?.Title));
                    break;
                case MapValidator.MapStatus.Removed:
                    context.SendMessage(BotMessages.Chat.MapManager.BeatmapRemoved);
                    break;
            }

            await HandleViolationAutoSkip();
            await context.Lobby.BehaviorEventProcessor!.OnBehaviorEvent("MapManagerInvalidMap");
        }

        [BotEvent(BotEventType.CommandExecuted, "Regulations")]
        public async Task OnRegulationsCommandExecuted(CommandEventContext commandEventContext)
        {
            var lobbyConfig = await context.Lobby.GetLobbyConfiguration();
            var timeSpan = TimeSpan.FromSeconds(Config.MaximumMapLength);

            var requiredModeName = lobbyConfig.Mode switch
            {
                GameMode.osu => "osu!std",
                GameMode.osuCatch => "osu!catch",
                GameMode.osuMania => "osu!mania",
                GameMode.osuTaiko => "osu!taiko",
                _ => "Any Mode"
            };

            commandEventContext.Reply(BotMessages.Chat.MapManager.Regulations(
                Config.MinimumStarRating,
                Config.MaximumStarRating,
                timeSpan,
                requiredModeName));
        }

        [BotEvent(BotEventType.CommandExecuted, "Mirror")]
        public void OnMirrorCommandExecuted(CommandEventContext commandEventContext)
        {
            commandEventContext.Reply(BotMessages.Chat.MapManager.Mirrors(Data.BeatmapInfo.SetId));
        }
        
        [BotEvent(BotEventType.CommandExecuted, "TimeLeft")]
        public async Task OnTimeLeftCommandExecuted(CommandEventContext commandEventContext)
        { 
            var finishTime = Data.MatchStartTime.Add(Data.BeatmapInfo.Length);
            
            // Add a few seconds to account for people loading/finishing the map
            finishTime = finishTime.AddSeconds(5);
            
            // If we know where the map actually starts (as in the break at the beginning of a song),
            // we can be a bit more clever and try to account for that.
            // This obviously assumes that the players skip, and skip at around 50% of that time.
            var beginSkipTime = await BeatmapProcessor.GetBeatmapStartTime(Data.BeatmapInfo.Id);
            if (beginSkipTime != null)
            {
                finishTime = finishTime.Subtract(TimeSpan.FromSeconds(beginSkipTime.Value * 0.5));
            }
            
            var timeLeft = (finishTime - DateTime.UtcNow).ToString(@"mm\:ss");

            if (DateTime.UtcNow >= finishTime)
            {
                timeLeft = "00:00";
            }
            
            // Allow the players to get pinged when the map is finished
            var pingEnabled = commandEventContext.Arguments.Length > 0 && commandEventContext.Arguments[0].Equals("ping", StringComparison.OrdinalIgnoreCase) && commandEventContext.Player != null;
            if (pingEnabled)
            {
                Data.MatchFinishMessageSubscribers.Add(commandEventContext.Player!.Name.ToIrcNameFormat());
            }
            
            commandEventContext.Reply(BotMessages.Chat.MapManager.TimeLeft(timeLeft, pingEnabled));
        }

        /// <summary>
        /// Will announce the beatmap in the chat, along with some information about it,
        /// such as star rating, status, length, bpm, etc.
        /// </summary>
        private async Task AnnounceBeatmap(BeatmapExtended beatmapModel, DifficultyAttributes difficultyAttributes, int mods)
        {
            var beatmapInfo = Data.BeatmapInfo;
            var starRatingRounded = Math.Round(difficultyAttributes.DifficultyRating, 2);
            var beatmapSet = beatmapModel.Set;
            
            context.SendMessage(BotMessages.Chat.MapManager.BeatmapHeaderWithMirrors(
                beatmapInfo.Id,
                beatmapInfo.SetId,
                beatmapSet?.Artist,
                beatmapSet?.Title,
                beatmapModel.Version));
            context.SendMessage(BotMessages.Chat.MapManager.BeatmapStatsSummary(
                starRatingRounded,
                beatmapModel.Status.ToString(),
                beatmapInfo.Length,
                beatmapModel.BPM));

            // If the bot has a performance point calculator, we can calculate the performance points for the beatmap.
            if (context.Lobby.Bot.PerformancePointService is { IsAvailable: true })
            {
                var ppInfo = await context.Lobby.Bot.PerformancePointService.CalculatePerformancePoints(beatmapInfo.Id, mods, beatmapModel.LastUpdated);
                if (ppInfo != null)
                {
                    context.SendMessage(BotMessages.Chat.MapManager.BeatmapDifficultyWithPp(
                        beatmapModel.ApproachRate,
                        beatmapModel.CircleSize,
                        beatmapModel.OverallDifficulty,
                        beatmapModel.HealthDrain,
                        (int)ppInfo.Performance100,
                        (int)ppInfo.Performance98,
                        (int)ppInfo.Performance95));

                    return;
                }
            }
         
            context.SendMessage(BotMessages.Chat.MapManager.BeatmapDifficultyWithoutPp(
                beatmapModel.ApproachRate,
                beatmapModel.CircleSize,
                beatmapModel.OverallDifficulty,
                beatmapModel.HealthDrain));
        }

        /// <summary>
        /// Will automatically skip the host if their violation count exceeds the maximum amount of violations.
        /// </summary>
        private async Task HandleViolationAutoSkip()
        {
            if (Config.AutomaticallySkipHostViolations == false ||
                Config.MaximumHostViolations == 0)
            {
                return;
            }

            Data.HostViolationCount++;

            if (Data.HostViolationCount < Config.MaximumHostViolations)
            {
                return;
            }

            Data.HostViolationCount = 0;

            var hostName = context.MultiplayerLobby.Host?.Name ?? "Desconocido";

            context.SendMessage(BotMessages.Chat.MapManager.SkippingHostForViolations(Config.MaximumHostViolations));
            context.Lobby.Bot.NotificationManager.Notify(
                BotMessages.Discord.HostSkippedForViolations(
                    context.Lobby.LobbyConfigurationId,
                    context.Channel,
                    hostName,
                    Config.MaximumHostViolations));
            
            await context.Lobby.BehaviorEventProcessor!.OnBehaviorEvent("HostQueueSkipHost");
        }

        /// <summary>
        /// Applies the provided beatmap id to the lobby, will also set the last bot applied beatmap id,
        /// so we don't end up in a loop of applying the same beatmap over and over.
        /// </summary>
        private async Task ApplyBeatmap(int beatmapId)
        {
            Data.LastBotAppliedBeatmapId = beatmapId;
            await context.ExecuteCommandAsync<MatchSetBeatmapCommand>([beatmapId.ToString(), "0"]);
        }

        /// <summary>
        /// This method will validate the map after we've just started the match.
        /// We do this to avoid any host trying to cheat the system, or to avoid any
        /// stupid double-time picks.
        /// </summary>
        private async Task ValidatePlayingMap()
        {
            if (Data.CurrentMapId == 0)
            {
                return;
            }
            
            await context.ExecuteCommandAsync<RoomSettingsUpdateCommand>();

            bool mapIsValid = !(!Config.AllowDoubleTime && (context.MultiplayerLobby.Mods & Mods.DoubleTime) != 0);

            List<string> mods = [];

            // We do this to only add the difficulty increasing mods, we don't really 
            // care about the rest, and therefore don't need to send it to the API.
                
            if ((context.MultiplayerLobby.Mods & Mods.DoubleTime) != 0 || 
                (context.MultiplayerLobby.Mods & Mods.Nightcore) != 0)
                mods.Add("DT");
            if ((context.MultiplayerLobby.Mods & Mods.HardRock) != 0)
                mods.Add("HR");
            if ((context.MultiplayerLobby.Mods & Mods.Hidden) != 0)
                mods.Add("HD");
                
            double calculatedSr = Data.BeatmapInfo.StarRating;
            var difficultyAttributesResult = await context.UsingApiClient(async (apiClient) => await apiClient.GetDifficultyAttributesAsync(Data.CurrentMapId, Ruleset.Osu, mods.ToArray()));
            if (difficultyAttributesResult.IsSuccess)
            {
                calculatedSr = difficultyAttributesResult.Value!.DifficultyRating;
                var lobbyConfig = await context.Lobby.GetLobbyConfiguration();
                var mapValidator = new MapValidator(context.Lobby, lobbyConfig, Config);
                var mapValidationResult = await mapValidator.ValidateBeatmap(difficultyAttributesResult.Value!, null);

                if (mapValidationResult != MapValidator.MapStatus.Ok)
                {
                    mapIsValid = false;
                }
            }

            if (mapIsValid)
            {
                // Everything checks out, we're done
                return;
            }

            var hostName = context.MultiplayerLobby.Host?.Name ?? "Desconocido";
                
            Log.Error(BotMessages.Logs.Behaviors.MapManagerInvalidPlayingMapAborted,
                context.Lobby.LobbyConfigurationId,
                Data.CurrentMapId,
                hostName);
            
            context.SendMessage(BotMessages.Chat.MapManager.InvalidPlayingMapAborted);

            var beatmapTitle = !string.IsNullOrWhiteSpace(Data.BeatmapInfo.Name)
                ? $"{Data.BeatmapInfo.Artist} - {Data.BeatmapInfo.Name}"
                : $"Beatmap #{Data.CurrentMapId}";

            context.Lobby.Bot.NotificationManager.Notify(
                BotMessages.Discord.InvalidMapMatchAborted(
                    context.Lobby.LobbyConfigurationId,
                    context.Channel,
                    hostName,
                    Data.CurrentMapId,
                    beatmapTitle,
                    context.MultiplayerLobby.Mods.ToAbbreviatedForm(true),
                    calculatedSr,
                    Config.MinimumStarRating,
                    Config.MaximumStarRating));

            await context.ExecuteCommandAsync<MatchAbortCommand>();
        }
    }
}
