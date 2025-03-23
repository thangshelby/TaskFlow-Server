using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MainService.Infras.Entities;

public class Sprint
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("date_started")]
    public DateTime DateStarted { get; set; }

    [BsonElement("date_ended")]
    public DateTime DateEnded { get; set; }

    [BsonElement("duration")]
    public int Duration { get; set; }

    [BsonElement("goal")]
    public string Goal { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("project_id")]
    public string ProjectId { get; set; } = string.Empty;
}