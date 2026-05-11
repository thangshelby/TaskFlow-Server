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

public class ProjectSummaryDomain
{
    public List<ProjectSummaryStatusCountDomain> ByStatus { get; set; } = new();
    public List<ProjectSummaryPriorityCountDomain> ByPriority { get; set; } = new();
    public List<ProjectSummaryTypeCountDomain> ByType { get; set; } = new();
    public List<ProjectSummaryContributorDomain> TopContributors { get; set; } = new();
    public List<ProjectSummaryTimelinePointDomain> Timeline { get; set; } = new();
    public int TotalIssues { get; set; }
    public int DoneIssues { get; set; }
    public int NewIssuesCount { get; set; }
    public int RecentlyUpdatedCount { get; set; }
}

public class ProjectSummaryStatusCountDomain
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ProjectSummaryPriorityCountDomain
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ProjectSummaryTypeCountDomain
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ProjectSummaryContributorDomain
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public int ResolvedCount { get; set; }
    public double ContributionPercent { get; set; }
}

public class ProjectSummaryTimelinePointDomain
{
    public DateTime Date { get; set; }
    public int DoneIssues { get; set; }
    public int RemainingScope { get; set; }
    public int AddedScope { get; set; }
}