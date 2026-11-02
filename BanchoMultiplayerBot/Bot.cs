using System.Text;
using BanchoMultiplayerBot.Bancho;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Database;
using BanchoMultiplayerBot.Interfaces;
using BanchoMultiplayerBot.Notifications;
using BanchoMultiplayerBot.Osu;
using BanchoMultiplayerBot.Utilities;
using Microsoft.EntityFrameworkCore;
using osu.NET;
using osu.NET.Authorization;
using Serilog;

namespace BanchoMultiplayerBot
{
    public class Bot(IBotConfiguration botConfiguration) : IAsyncDisposable
    {
        public List<ILobby> Lobbies { get; } = [];
        
        public BanchoConnection BanchoConnection { get; } = new(botConfiguration.BanchoClientConfiguration);

        public OsuApiClient OsuApiClient { get; } = new(new OsuClientAccessTokenProvider(botConfiguration.OsuApiClientId, botConfiguration.OsuApiClientSecret), new OsuApiClientOptions(), null!);

        public PerformancePointService? PerformancePointService { get; } = new(botConfiguration.PerformancePointServiceUrl, botConfiguration.BeatmapCacheDirectory);

        public NotificationManager NotificationManager { get; } = new(botConfiguration);
        
        public IBotConfiguration BotConfiguration => _botConfiguration;
        
        public event Action<ILobby>? OnLobbyCreated;
        public event Action<ILobby>? OnLobbyRemoved;
        
        private readonly IBotConfiguration _botConfiguration = botConfiguration;
        private CommandProcessor? _commandProcessor;

        public async Task StartAsync()
        {
            var username = _botConfiguration.BanchoClientConfiguration.Username;
            Log.Information(BotMessages.Logs.Bot.Starting, username);
            
            NotificationManager.Notify(BotMessages.Discord.BotStarting(
                username,
                PerformancePointService?.IsAvailable == true,
                _botConfiguration.PerformancePointServiceUrl));
            
            _commandProcessor ??= new CommandProcessor(this);
            _commandProcessor.Start();

            BanchoConnection.OnReady += OnBanchoReady;
            BanchoConnection.OnConnectionError += OnConnectionError;
            BanchoConnection.OnReconnected += OnBanchoReconnected;
            BanchoConnection.OnReconnectExhausted += OnBanchoReconnectExhausted;

            await LoadLobbiesFromDatabase();
            await BanchoConnection.StartAsync();
        }

        public async Task StopAsync()
        {
            Log.Information(BotMessages.Logs.Bot.ShuttingDown, Lobbies.Count);
            NotificationManager.Notify(BotMessages.Discord.BotShuttingDown(Lobbies.Count));
            
            BanchoConnection.OnReady -= OnBanchoReady;
            BanchoConnection.OnConnectionError -= OnConnectionError;
            BanchoConnection.OnReconnected -= OnBanchoReconnected;
            BanchoConnection.OnReconnectExhausted -= OnBanchoReconnectExhausted;
            
            _commandProcessor?.Stop();
            
            foreach (var lobby in Lobbies)
            {
                await lobby.Dispose();
            }

            await BanchoConnection.StopAsync();
        }
        
        public async Task ReloadLobbies()
        {
            if (!BanchoConnection.IsConnected)
            {
                throw new InvalidOperationException("Bot is not connected to Bancho.");
            }
            
            await LoadLobbiesFromDatabase();

            foreach (var lobby in Lobbies.Where(lobby => !(lobby.Health == LobbyHealth.Ok || lobby.Health == LobbyHealth.Idle)))
            {
                await lobby.ConnectAsync();
            }
        }
        
        public async Task DeleteLobby(int configurationId)
        {
            await using var context = new BotDbContext();
            
            var lobby = Lobbies.FirstOrDefault(x => x.LobbyConfigurationId == configurationId);

            if (lobby == null)
            {
                throw new InvalidOperationException("Failed to find lobby configuration.");
            }

            await lobby.Dispose();
            
            Lobbies.Remove(lobby);
            OnLobbyRemoved?.Invoke(lobby);

            context.Remove(lobby);
            
            await context.SaveChangesAsync();

            Log.Information(BotMessages.Logs.Bot.LobbyDeleted, configurationId);
        }
        
        private async Task LoadLobbiesFromDatabase()
        {
            await using var context = new BotDbContext();
         
            Log.Verbose(BotMessages.Logs.Bot.LoadingLobbyConfigurations);

            var lobbyConfigurations = await context.LobbyConfigurations.ToListAsync();
            
            foreach (var lobbyConfiguration in lobbyConfigurations.Where(lobbyConfiguration => !Lobbies.Any(x => x.LobbyConfigurationId == lobbyConfiguration.Id)))
            {
                var lobby = new Lobby(this, lobbyConfiguration.Id);
                
                Lobbies.Add(lobby);
                
                OnLobbyCreated?.Invoke(lobby);
                
                Log.Information(BotMessages.Logs.Bot.LoadedLobbyConfiguration, lobbyConfiguration.Id, lobbyConfiguration.Name);
            }

            Lobbies.Sort((x, y) => x.LobbyConfigurationId.CompareTo(y.LobbyConfigurationId));
        }

        private async void OnBanchoReady()
        {
            var summaryBuilder = new StringBuilder();
            var activeCount = 0;

            foreach (var lobby in Lobbies)
            {
                Log.Information(BotMessages.Logs.Bot.StartingLobby, lobby.LobbyConfigurationId);
                
                await lobby.ConnectAsync();
                
                var attempts = 0;
                while (!(lobby.Health == LobbyHealth.Ok || lobby.Health == LobbyHealth.Idle))
                {
                    if (attempts++ > 20)
                    {
                        Log.Error(BotMessages.Logs.Bot.LobbyReadyTimeout, lobby.LobbyConfigurationId, lobby.Health);
                        lobby.Health = LobbyHealth.ChannelCreationFailed;

                        var failedConfig = await lobby.GetLobbyConfiguration();
                        NotificationManager.Notify(BotMessages.Discord.LobbyCreationTimeout(lobby.LobbyConfigurationId, failedConfig.Name));
                        break;
                    }

                    // If something went wrong with the connection, we should stop trying.
                    if (BanchoConnection.ConnectionCancellationToken?.IsCancellationRequested == true)
                    {
                        break;
                    }
                    
                    await Task.Delay(500);
                }

                var config = await lobby.GetLobbyConfiguration();
                var channel = lobby.MultiplayerLobby?.ChannelName ?? "N/A";
                var isReady = lobby.Health is LobbyHealth.Ok or LobbyHealth.Idle;

                if (isReady)
                {
                    activeCount++;
                }

                summaryBuilder.AppendLine($"• **#{lobby.LobbyConfigurationId} {config.Name}** — `{channel}` (`{lobby.Health}`)");
            }
            
            NotificationManager.Notify(BotMessages.Discord.BotReady(
                _botConfiguration.BanchoClientConfiguration.Username,
                activeCount,
                Lobbies.Count,
                summaryBuilder.ToString().TrimEnd()));
        }

        private void OnConnectionError()
        {
            var banchoConfig = _botConfiguration.BanchoClientConfiguration;
            NotificationManager.Notify(BotMessages.Discord.BanchoConnectionLost(
                banchoConfig.Username,
                Lobbies.Count,
                banchoConfig.BanchoReconnectDelay,
                banchoConfig.BanchoReconnectAttempts));
        }

        private void OnBanchoReconnected(int attempt, int maxAttempts)
        {
            NotificationManager.Notify(BotMessages.Discord.BanchoReconnected(
                _botConfiguration.BanchoClientConfiguration.Username,
                attempt,
                maxAttempts));
        }

        private void OnBanchoReconnectExhausted(int maxAttempts)
        {
            var banchoConfig = _botConfiguration.BanchoClientConfiguration;
            NotificationManager.Notify(BotMessages.Discord.BanchoReconnectExhausted(
                banchoConfig.Username,
                maxAttempts,
                banchoConfig.BanchoReconnectAttemptDelay));
        }
        
        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            await Task.WhenAny(StopAsync(), Task.Delay(5000));
        }
    }
}
