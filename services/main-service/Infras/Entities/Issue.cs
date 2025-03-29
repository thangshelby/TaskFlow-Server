using MainService.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MainService.Infras.Entities;

public class Issue
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("project_id")]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("sprint_id")]
    public string SprintId { get; set; } = string.Empty;

    [BsonElement("assignee_id")]
    public string AssigneeId { get; set; } = string.Empty;

    [BsonElement("parent_id")]
    public string ParentId { get; set; } = string.Empty;

    [BsonElement("reporter_id")]
    public string ReporterId { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public IssueType Type { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public IssueStatus Status { get; set; }

    [BsonElement("priority")]
    [BsonRepresentation(BsonType.String)]
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    [BsonElement("summary")]
    public string Summary { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("story_point")]
    public int StoryPoint { get; set; }

    [BsonElement("attachments")]
    public List<string> Attachments { get; set; } = new();

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

