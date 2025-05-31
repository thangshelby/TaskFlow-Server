namespace MainService.Domain.Entities;

public class ActivityChange
{
    public string? Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
public class ActivityDomain
{
    public string? Id { get; set; }
    public required string IssueId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ActionType { get; set; }
    public List<ActivityChange>? Changes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class ActivityAction
{
    public const string ISSUE_UPDATED = "ISSUE_UPDATED";
    public const string ISSUE_CREATED = "ISSUE_CREATED";
}