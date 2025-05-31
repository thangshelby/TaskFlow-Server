using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MainService.Domain.Entities;

namespace MainService.Infras.Entities;

public class ActivityChangeEntity
{
    [BsonElement("field")]
    public string? Field { get; set; }

    [BsonElement("old_value")]
    public string? OldValue { get; set; }

    [BsonElement("new_value")]
    public string? NewValue { get; set; }
}
public class Activity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("issue_id")]
    public string IssueId { get; set; } = default!;

    [BsonElement("user_id")]
    public string? UserId { get; set; }

    [BsonElement("user_name")]
    public string? UserName { get; set; }

    [BsonElement("action_type")]
    public string? ActionType { get; set; }

    [BsonElement("changes")]
    public List<ActivityChangeEntity>? Changes { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static Activity FromDomain(ActivityDomain domain)
    {
        return new Activity
        {
            Id = domain.Id,
            IssueId = domain.IssueId,
            UserId = domain.UserId,
            ActionType = domain.ActionType,
            Changes = domain.Changes?.Select(change => new ActivityChangeEntity
            {
                Field = change.Field,
                OldValue = change.OldValue,
                NewValue = change.NewValue
            }).ToList(),
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }
}
