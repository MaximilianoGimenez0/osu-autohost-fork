using BanchoMultiplayerBot.Bancho.Data;
using BanchoMultiplayerBot.Bancho.Interfaces;
using BanchoMultiplayerBot.Constants;
using BanchoSharp;
using Prometheus;
using Serilog;
using BanchoSharp.Interfaces;

namespace BanchoMultiplayerBot.Bancho
{
    /// <summary>
    /// Main class for handling the connection to Bancho within BanchoMultiplayerBot.Bancho.
    /// </summary>
    public class BanchoConnection : IBanchoConnection
    {
        /// <summary>
        /// If the connection to Bancho is currently active.
        /// </summary>
        public bool IsConnected 
        {
            get => _isConnected;
            
            private set
            {
                _isConnected = value;
                ConnectionHealth.Set(_isConnected ? 1 : 0);
            }    
        }

        /// <summary>
        /// The underlying BanchoSharp Bancho client 
        /// </summary>
        public IBanchoClient? BanchoClient { get; private set; }

        /// <summary>
        /// Handler for incoming and outgoing messages to Bancho
        /// </summary>
        public IMessageHandler MessageHandler { get; }
        
        /// <summary>
        /// Handler for executing bancho multiplayer commands, in a more structured way.
        /// </summary>
        public ICommandHandler CommandHandler { get; }
        
        /// <summary>
        /// Handler for managing channels that we are connected to.
        /// </summary>
        public IChannelHandler ChannelHandler { get; }
        
        /// <summary>
        /// Invoked whenever a connection to Bancho is ready.
        /// </summary>
        public event Action? OnReady;
        
        /// <summary>
        /// Invoked whenever a connection to Bancho has an error, however
        /// the bancho connection will attempt to reconnect automatically.
        /// </summary>
        public event Action? OnConnectionError;

        /// <summary>
        /// Invoked whenever a lost connection to Bancho is re-established.
        /// Parameters are (currentAttempt, maxAttempts).
        /// </summary>
        public event Action<int, int>? OnReconnected;

        /// <summary>
        /// Invoked whenever all reconnection attempts to Bancho have been exhausted.
        /// Parameter is maxAttempts.
        /// </summary>
        public event Action<int>? OnReconnectExhausted;

        /// <summary>
        /// Used to cancel the connection to Bancho.
        /// </summary>
        public CancellationToken? ConnectionCancellationToken => _cancellationTokenSource?.Token;

        private readonly BanchoClientConfiguration _banchoConfiguration;
        private ConnectionHandler? _connectionWatchdog;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isConnected;

        private static readonly Gauge ConnectionHealth = Metrics.CreateGauge("bot_connection_health", "Bancho connection health");

        public BanchoConnection(BanchoClientConfiguration banchoClientConfiguration) 
        {
            _banchoConfiguration = banchoClientConfiguration;

            MessageHandler = new MessageHandler(this, banchoClientConfiguration);
            ChannelHandler = new ChannelHandler(this);
            CommandHandler = new CommandHandler(MessageHandler, banchoClientConfiguration);
        }

        /// <summary>
        /// Attempt to connect to Bancho, this is non-blocking, and will
        /// return immediately.
        /// </summary>
        public Task StartAsync()
        {
            _ = Task.Run(ConnectAsync);

            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Stop the connection to Bancho, this is blocking, and will
        /// return when the connection is fully terminated.
        /// </summary>
        public async Task StopAsync()
        {
            await DisconnectAsync();
        }
        
        private async Task ConnectAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // Make sure we've fully disconnected before continuing,
            // we more or less want a fully reset state.
            await DisconnectAsync();

            // We set up the BanchoClient here without any logging and such
            // since we deal with that ourselves.
            BanchoClient = new BanchoClient(
                                    new BanchoClientConfig(new IrcCredentials(_banchoConfiguration.Username, _banchoConfiguration.Password), 
                                    LogLevel.None,
                                    false));

            BanchoClient.OnAuthenticated += BanchoOnAuthenticated;
            
            try
            {
                Log.Information(BotMessages.Logs.Bancho.Connecting, _banchoConfiguration.Username);
                
                // This call is blocking and only return once the connection is
                // terminated, so ideally we'll stay here till something goes wrong.
                await BanchoClient.ConnectAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, BotMessages.Logs.Bancho.ConnectionException, _banchoConfiguration.Username);
                return;
            }

            Log.Information(BotMessages.Logs.Bancho.ConnectionTerminated);
        }

        private async Task DisconnectAsync()
        {
            if (_connectionWatchdog?.IsRunning == true)
            {
                _connectionWatchdog.OnConnectionLost -= OnConnectionLost;
                _connectionWatchdog.Stop();
                _connectionWatchdog = null;
            }

            if (MessageHandler.IsRunning)
            {
                MessageHandler.Stop();
            }

            ChannelHandler.Stop();

            var hadActiveClient = BanchoClient != null;

            if (BanchoClient != null)
            {
                Log.Information(BotMessages.Logs.Bancho.Disconnecting);
                
                await BanchoClient.DisconnectAsync();

                BanchoClient.OnAuthenticated -= BanchoOnAuthenticated;
                BanchoClient.TcpClient?.Dispose();
                BanchoClient?.Dispose();
            }

            BanchoClient = null;
            IsConnected = false;

            if (hadActiveClient)
            {
                Log.Information(BotMessages.Logs.Bancho.Disconnected);
            }
        }

        private void BanchoOnAuthenticated()
        {
            if (BanchoClient?.TcpClient == null)
            {
                // Shouldn't ever happen, hopefully.
                return;
            }

            Log.Information(BotMessages.Logs.Bancho.Authenticated, _banchoConfiguration.Username);
            
            IsConnected = true;

            // Once we got a connection successfully up and running, make sure to initiate
            // the connection watchdog immediately
            _connectionWatchdog = new ConnectionHandler(BanchoClient.TcpClient, MessageHandler);
            _connectionWatchdog.OnConnectionLost += OnConnectionLost;
            _connectionWatchdog.Start();

            ChannelHandler.Start();
            MessageHandler.Start();
            
            OnReady?.Invoke();
        }

        private async void OnConnectionLost()
        {
            Log.Error(BotMessages.Logs.Bancho.ConnectionLostReconnecting, _banchoConfiguration.BanchoReconnectDelay);
            
            IsConnected = false;
            
            _cancellationTokenSource?.Cancel();
            
            OnConnectionError?.Invoke();
            
            // Wait a bit before attempting to reconnect
            await Task.Delay(_banchoConfiguration.BanchoReconnectDelay * 1000);

            int connectionAttempts = 0;
            while (connectionAttempts < _banchoConfiguration.BanchoReconnectAttempts)
            {
                var currentAttempt = connectionAttempts + 1;
                Log.Information(BotMessages.Logs.Bancho.ReconnectAttempt, currentAttempt, _banchoConfiguration.BanchoReconnectAttempts);

                _ = Task.Run(ConnectAsync);

                // In normal circumstances, we should be able to reconnect within 10 seconds
                await Task.Delay(10000);

                // If we're back in action, IsConnected will be true
                // we can safely exit due to a new watchdog being started
                // so even if we lose connection again, we'll be able to
                // reconnect.
                if (IsConnected)
                {
                    Log.Information(BotMessages.Logs.Bancho.ReconnectedSuccessfully, currentAttempt, _banchoConfiguration.BanchoReconnectAttempts);
                    OnReconnected?.Invoke(currentAttempt, _banchoConfiguration.BanchoReconnectAttempts);

                    return;
                }
                
                Log.Error(
                    BotMessages.Logs.Bancho.ReconnectAttemptFailed,
                    currentAttempt,
                    _banchoConfiguration.BanchoReconnectAttempts,
                    _banchoConfiguration.BanchoReconnectAttemptDelay);

                await Task.Delay(_banchoConfiguration.BanchoReconnectAttemptDelay * 1000);

                connectionAttempts++;
            }

            Log.Fatal(BotMessages.Logs.Bancho.ReconnectExhaustedFatal, _banchoConfiguration.BanchoReconnectAttempts);
            OnReconnectExhausted?.Invoke(_banchoConfiguration.BanchoReconnectAttempts);

            DisconnectAsync().Wait(TimeSpan.FromSeconds(30));
        }
    }
}
