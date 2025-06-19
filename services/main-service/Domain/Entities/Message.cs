using MainService.Domain.Entities;
public class KafkaMessage<T>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string EventType { get; set; }
    public required T Data { get; set; }

    [System.Text.Json.Serialization.JsonExtensionData]
    public Dictionary<string, object>? extra { get; set; }
}

public enum KafkaMessageAction
{
    ACTIVITIES_ISSUE_CHANGED,
    ACTIVITIES_ISSUE_CREATED,

    NOTIFICATIONS_CREATE_ISSUE
}

public readonly record struct TopicName(string Value)
{
    public static readonly TopicName NOTIFICATIONS = new("notifications");
    public static readonly TopicName ACTIVITIES = new("activities");

    public override string ToString() => Value;
}
public class IActivitiesMessage
{
    public IssueDomain? OldIssue { get; set; }
    public required IssueDomain NewIssue { get; set; }
    public required string UserId { get; set; }
};

public class INotificationMessage
{
    public required string RecipientId { get; set; }
    public string? ActorId { get; set; }
    public string? IssueId { get; set; }
    public required string Type { get; set; }
};

public enum NotificationType
{
    ASSIGNMENT,
    MENTION,
    COMMENT,
    STATUS_UPDATE,
    DUE_DATE_REMINDER,
    PROJECT_INVITATION,
    REACTION,
    SYSTEM_ALERT,
    SPRINT_STARTED,
    PROJECT_ADDED,
    PROJECT_TEAM_ADDED,
}