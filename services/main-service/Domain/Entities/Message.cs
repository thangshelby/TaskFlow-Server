using System.Text.Json;
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
    public IssueDomain? OldIssue { get; set; }
    public required IssueDomain NewIssue { get; set; }
    public required string EventType { get; set; }
    public required string UserId { get; set; }
};