using System.Collections.Concurrent;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Interfaces;
using Serilog;

namespace BanchoMultiplayerBot.Notifications;

public class NotificationManager
{
    private readonly List<INotificationProvider> _notificationProviders = [];
    private readonly ConcurrentDictionary<string, DateTime> _deduplicationCache = new();

    public NotificationManager(IBotConfiguration botConfiguration)
    {
        if (!string.IsNullOrWhiteSpace(botConfiguration.DiscordWebhookUrl) &&
            Uri.TryCreate(botConfiguration.DiscordWebhookUrl, UriKind.Absolute, out var webhookUri) &&
            (webhookUri.Scheme == Uri.UriSchemeHttp || webhookUri.Scheme == Uri.UriSchemeHttps))
        {
            _notificationProviders.Add(new DiscordNotificationProvider(botConfiguration.DiscordWebhookUrl));
        }
        else
        {
            Log.Information(BotMessages.Logs.Notifications.WebhookUrlNotConfigured);
        }
    }

    public void Notify(string title, string message, NotificationSeverity severity = NotificationSeverity.Info)
    {
        Notify(new NotificationMessage
        {
            Title = title,
            Description = message,
            Severity = severity
        });
    }

    public void Notify(NotificationMessage notification)
    {
        if (_notificationProviders.Count == 0)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(notification.DeduplicationKey))
        {
            var window = notification.DeduplicationWindow ?? TimeSpan.FromMinutes(3);
            var now = DateTime.UtcNow;

            if (_deduplicationCache.TryGetValue(notification.DeduplicationKey, out var lastSent) &&
                now - lastSent < window)
            {
                Log.Verbose(BotMessages.Logs.Notifications.NotificationDeduplicated, notification.DeduplicationKey, notification.Title);
                return;
            }

            _deduplicationCache[notification.DeduplicationKey] = now;
        }

        // Fire and forget, don't want to block/expect await from the caller
        Task.Run(async () =>
        {
            foreach (var provider in _notificationProviders)
            {
                try
                {
                    await provider.NotifyAsync(notification);
                }
                catch (Exception e)
                {
                    Log.Error(
                        e,
                        BotMessages.Logs.Notifications.NotificationSendFailed,
                        notification.Title,
                        provider.GetType().Name,
                        e.Message);
                }
            }
        });
    }
}
