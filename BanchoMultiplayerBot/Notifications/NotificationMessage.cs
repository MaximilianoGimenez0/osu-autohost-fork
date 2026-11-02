namespace BanchoMultiplayerBot.Notifications;

public class NotificationMessage
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public NotificationSeverity Severity { get; init; } = NotificationSeverity.Info;

    public string? Url { get; init; }

    public string? ThumbnailUrl { get; init; }

    public string? FooterText { get; init; }

    public string? DeduplicationKey { get; init; }

    public TimeSpan? DeduplicationWindow { get; init; }

    public IReadOnlyList<NotificationField> Fields { get; init; } = [];
}

