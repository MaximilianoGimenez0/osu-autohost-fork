using System.Text;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Interfaces;
using Newtonsoft.Json;
using Serilog;

namespace BanchoMultiplayerBot.Notifications;

public class DiscordNotificationProvider(string targetUrl) : INotificationProvider
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public Task NotifyAsync(NotificationMessage notification) => SendDiscordNotification(targetUrl, notification);

    private static async Task SendDiscordNotification(string url, NotificationMessage notification)
    {
        var embed = new Dictionary<string, object>
        {
            ["title"] = notification.Title,
            ["description"] = notification.Description,
            ["color"] = GetColorForSeverity(notification.Severity),
            ["timestamp"] = DateTime.UtcNow.ToString("O"),
            ["footer"] = new
            {
                text = notification.FooterText ?? BotMessages.Discord.DefaultFooter
            }
        };

        if (!string.IsNullOrWhiteSpace(notification.Url))
        {
            embed["url"] = notification.Url;
        }

        if (!string.IsNullOrWhiteSpace(notification.ThumbnailUrl))
        {
            embed["thumbnail"] = new
            {
                url = notification.ThumbnailUrl
            };
        }

        if (notification.Fields.Count > 0)
        {
            embed["fields"] = notification.Fields
                .Select(f => new
                {
                    name = f.Name,
                    value = string.IsNullOrWhiteSpace(f.Value) ? "-" : f.Value,
                    inline = f.Inline
                })
                .ToList();
        }

        var payload = new
        {
            embeds = new List<object> { embed }
        };

        using var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        using var response = await HttpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            Log.Error(
                BotMessages.Logs.Notifications.DiscordHttpError,
                (int)response.StatusCode,
                notification.Title,
                responseBody);
        }
    }

    private static int GetColorForSeverity(NotificationSeverity severity) => severity switch
    {
        NotificationSeverity.Info => 0x3E97E6,
        NotificationSeverity.Success => 0x2ECC71,
        NotificationSeverity.Warning => 0xF39C12,
        NotificationSeverity.Error => 0xE74C3C,
        NotificationSeverity.Critical => 0x992D22,
        NotificationSeverity.Moderation => 0x9B59B6,
        NotificationSeverity.Gameplay => 0xF1C40F,
        _ => 0x3E97E6
    };
}
