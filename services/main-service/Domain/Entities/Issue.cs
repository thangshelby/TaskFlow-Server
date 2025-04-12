namespace MainService.Domain.Entities;

public class IssueDomain
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string? SprintId { get; set; }
    public string? AssigneeId { get; set; }
    public string ParentId { get; set; } = string.Empty;
    public string ReporterId { get; private set; } = string.Empty;
    public IssueType Type { get; set; }
    public IssueStatus Status { get; set; }
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StoryPoint { get; set; }
    public List<string> Attachments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public IssueDomain(string projectId, string reporterId)
    {
        ProjectId = projectId;
        ReporterId = reporterId;
    }

    public void AssignToSprint(string? sprintId)
    {
        SprintId = sprintId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignToUser(string? assigneeId)
    {
        AssigneeId = assigneeId;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum IssueType
{
    Bug,
    Task,
    Story,
    Epic
}

public enum IssueStatus
{
    ToDo,
    InProgress,
    Done,
    Closed
}

public enum IssuePriority
{
    Low,
    Medium,
    High,
    Critical
}
