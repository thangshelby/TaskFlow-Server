using MainService.Domain.Entities;
using MainService.Domain.Enums;
namespace MainService.Domain.Interfaces;
public interface IIssueRepository
{
    Task<IssueDomain> CreateIssue(IssueDomain issue);
    Task<IssueDomain> GetIssue(string id);
    Task<IssueDomain> UpdateIssue(UpdateIssueParams issue);
    Task DeleteIssue(string id);
    Task<(List<IssueDomain> Issues, int TotalCount)> ListIssues(GetIssuesParams param);
}

public class CreateIssueParams
{
    public required string ProjectId;
    public required string Title;
    public required string ReporterId;
    public required string Status;
    public string? SprintId;
    public string? AssigneeId;
}
public class GetIssuesParams
{
    public string? ProjectId;
    public string? Status;
    public string? AssigneeId;
    public string? SprintId;
    public string? Keyword;
    public int Page;
    public int Limit;
}
public class UpdateIssueParams
{
    public required string IssueId;
    public string? Title;
    public string? ProjectId;
    public string? SprintId;
    public string? AssigneeId;
    public string? Description;
    public string? Summary;
    public int? StoryPoint;
    public string? ReporterId;
    public string? Status;
    public string? ParentId;
    public IssueType? Type;
    public IssuePriority? Priority;
    public List<string>? Attachments; 
}