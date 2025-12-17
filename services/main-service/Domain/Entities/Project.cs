using MainService.Domain.Enums;
namespace MainService.Domain.Entities;

public class ProjectDomain
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public ProjectAccess Access { get; set; }
    public ProjectType Type { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<ProjectMemberDomain>? ProjectMembers { get; set; } = new List<ProjectMemberDomain>();
    
    // New fields
    public int? IssuesCount { get; set; }
    public int? MembersCount { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? Description { get; set; }
    public string? BackgroundImg { get; set; }
}

public class ProjectColumnDomain
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string ProjectId { get; set; } = string.Empty;
    public List<string> IssueIds { get; set; } = new List<string>();
    public List<IssueDomain>? Issues { get; set; } = new List<IssueDomain>();
    public int Order { get; set; } = 0;
}