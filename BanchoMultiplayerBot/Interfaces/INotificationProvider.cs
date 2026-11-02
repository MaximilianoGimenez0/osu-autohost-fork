using BanchoMultiplayerBot.Notifications;

namespace BanchoMultiplayerBot.Interfaces;

public interface INotificationProvider
{
    public Task NotifyAsync(NotificationMessage notification);
}
