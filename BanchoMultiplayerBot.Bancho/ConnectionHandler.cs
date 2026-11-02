using BanchoMultiplayerBot.Bancho.Interfaces;
using BanchoMultiplayerBot.Constants;
using Serilog;
using System.Net.Sockets;

namespace BanchoMultiplayerBot.Bancho
{
    internal class ConnectionHandler(TcpClient tcpClient, IMessageHandler messageHandler) : IConnectionHandler
    {
        public bool IsRunning { get; private set; } = false;

        public event Action? OnConnectionLost;

        private readonly TcpClient _tcpClient = tcpClient;
        private readonly IMessageHandler _messageHandler = messageHandler;

        private Task? _watchdogTask = null!;
        private bool _exitRequested = false;

        private DateTime _lastMessageReceived = DateTime.UtcNow;
        private DateTime _lastTestMessageSent = DateTime.UtcNow;

        public void Start()
        {
            Log.Verbose(BotMessages.Logs.Bancho.WatchdogStarting);

            _exitRequested = false;
            _watchdogTask = Task.Run(ConnectionWatchdogTask);
        }

        public void Stop()
        {
            Log.Verbose(BotMessages.Logs.Bancho.WatchdogStopping);

            _exitRequested = true;

            if (_watchdogTask == null || _watchdogTask.Status == TaskStatus.RanToCompletion || _watchdogTask.Status == TaskStatus.Faulted || _watchdogTask.Status == TaskStatus.Canceled)
            {
                Log.Warning(BotMessages.Logs.Bancho.WatchdogNotRunningOnStop, _watchdogTask?.Status.ToString() ?? "null");
                
                _watchdogTask = null;

                return;
            }

            _watchdogTask.Wait();
            _watchdogTask = null;
        }

        private async Task ConnectionWatchdogTask()
        {
            Log.Information(BotMessages.Logs.Bancho.WatchdogStarted);

            IsRunning = true;

            _messageHandler.OnMessageReceived += MessageHandler_OnMessageReceived;

            while (!_exitRequested)
            {
                await Task.Delay(1000);

                if (IsConnectionHealthy())
                {
                    continue;
                }

                Log.Error(BotMessages.Logs.Bancho.WatchdogDetectedLostConnection);

                // We don't want whatever callback where invoking to be blocking this task,
                // so invoke the event in another task.
                _ = Task.Run(() =>
                {
                    OnConnectionLost?.Invoke();
                });

                break;
            }

            Log.Verbose(BotMessages.Logs.Bancho.WatchdogStopped);

            _messageHandler.OnMessageReceived -= MessageHandler_OnMessageReceived;

            IsRunning = false;
        }

        private bool IsConnectionHealthy()
        {
            // First check if the TCP socket seems healthy
            if (!IsTcpConnectionAlive(_tcpClient))
            {
                Log.Error(BotMessages.Logs.Bancho.WatchdogTcpUnhealthy);

                return false;
            }

            // Additional fail-safe message check
            if (!((DateTime.UtcNow - _lastMessageReceived).TotalMinutes > 5))
            {
                return true;
            }
            
            // We realistically should be receiving at least one message in 5 minutes during normal operation, so if we haven't
            // attempted to send a dummy message to see if the connection is still alive. We won't be signaling that the connection
            // is lost, since we're not sure if it's actually lost or not.
            if ((DateTime.UtcNow - _lastTestMessageSent).TotalMinutes > 5)
            {
                Log.Warning(BotMessages.Logs.Bancho.WatchdogSendingKeepAlive);

                _messageHandler.SendMessage("BanchoBot", "dummy message");  
                
                _lastTestMessageSent = DateTime.UtcNow;
            }

            return true;
        }

        private void MessageHandler_OnMessageReceived(BanchoSharp.Interfaces.IPrivateIrcMessage obj)
        {
            _lastMessageReceived = DateTime.UtcNow;
        }

        // See https://stackoverflow.com/a/6993334
        private static bool IsTcpConnectionAlive(TcpClient? client)
        {
            try
            {
                if (client != null && client.Client.Connected)
                {
                    if (!client.Client.Poll(0, SelectMode.SelectRead))
                    {
                        return true;
                    }

                    byte[] buff = new byte[1];

                    return client.Client.Receive(buff, SocketFlags.Peek) != 0;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
