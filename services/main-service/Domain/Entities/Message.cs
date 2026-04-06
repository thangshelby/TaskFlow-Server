using MainService.Domain.Entities;
public class QueueMessage<T>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string EventType { get; set; }
    public required T Data { get; set; }

    [System.Text.Json.Serialization.JsonExtensionData]
    public Dictionary<string, object>? extra { get; set; }
}
// [TOPIC NAME]_[ACTION]
public enum QueueMessageAction
{
    ACTIVITIES_ISSUE_CHANGED,
    ACTIVITIES_ISSUE_CREATED,
    ACTIVITIES_PROJECT_CREATED,

    ACTIVITIES_MEMBER_JOINED_PROJECT,

    NOTIFICATIONS_CREATE_NEW_NOTIFICATION,

    MAILS_SEND_VERIFY_OTP_USER,

    MAILS_SEND_ACTIVITIES_CREATED,

    METADATA_CREATE_PRESIGNED_URL_IMAGE,
}

public readonly record struct QueueTopicName(string Value)
{
    public static readonly QueueTopicName NOTIFICATIONS = new("notifications");
    public static readonly QueueTopicName ACTIVITIES = new("activities");
    public static readonly QueueTopicName MAILS = new("mails");
    public static readonly QueueTopicName METADATA = new("metadata");

    public override string ToString() => Value;
}
public class IMailMessage
{   
    public object? Data { get; set; }
    public required string UserId { get; set; }
};
public class IActivitiesMessage
{
    public IssueDomain? OldIssue { get; set; }
    public required IssueDomain NewIssue { get; set; }
    public required string UserId { get; set; }
};

public class IProjectMessage
{
    public required string ProjectId { get; set; }
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