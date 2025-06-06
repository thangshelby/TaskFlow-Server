using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MainService.Domain.Entities;
using MainService.Domain.Enums;

namespace MainService.Infras.Entities;

public class NotificationEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("user_id")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("type")]
    public NotificationType Type { get; set; }

    [BsonElement("reference_id")]
    public string ReferenceId { get; set; } = string.Empty;

    [BsonElement("is_read")]
    public bool IsRead { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static NotificationEntity FromDomain(NotificationMessageDomain domain)
    {
        return new NotificationEntity
        {
            Id = domain.Id,
            UserId = domain.UserId,
            Title = domain.Title,
            Content = domain.Content,
            Type = domain.Type,
            ReferenceId = domain.ReferenceId,
            IsRead = domain.IsRead,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }
}