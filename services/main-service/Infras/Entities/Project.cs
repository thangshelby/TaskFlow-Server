using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MainService.Infras.Entities;

public class Project
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("key")]
    public string Key { get; set; } = string.Empty;

    [BsonElement("access")]
    public string Access { get; set; } = "PUBLIC";

    [BsonElement("type")]
    public string Type { get; set; } = "Scrum";

    [BsonElement("owner_id")]
    public string OwnerId { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("team_members")]
    public List<ProjectMember>? ProjectMembers { get; set; } = new List<ProjectMember>();

    // New fields
    [BsonElement("issues_count")]
    public int? IssuesCount { get; set; }

    [BsonElement("members_count")]
    public int? MembersCount { get; set; }

    [BsonElement("due_date_from")]
    public DateTime? DueDateFrom { get; set; }

    [BsonElement("due_date_to")]
    public DateTime? DueDateTo { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("background_img")]
    public string? BackgroundImg { get; set; }
}

[BsonIgnoreExtraElements]
public class ProjectColumn
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("project_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("issue_ids")]
    public List<ObjectId> IssueIds { get; set; } = new List<ObjectId>();

    [BsonElement("issues")]
    public List<Issue>? Issues { get; set; }

    [BsonElement("order")]
    public int Order { get; set; } = 0;
}