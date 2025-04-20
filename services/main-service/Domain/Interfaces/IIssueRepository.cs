using MainService.Domain.Entities;
namespace MainService.Domain.Interfaces;
public interface IIssueRepository
{
    Task<IssueDomain> CreateIssue(IssueDomain issue);
    Task<IssueDomain> GetIssue(string id);
    Task<IssueDomain> UpdateIssue(IssueDomain issue);
    Task DeleteIssue(string id);
    Task<(List<IssueDomain> Issues, int TotalCount)> ListIssues(GetIssuesParams param);
}

public class CreateProjectParams
{
    public required string ProjectId;
    public required string Title;
    public required string ReporterId;
    public required string ColumnId;
    public string? SprintId;
    public string? AssigneeId;
}
public class GetIssuesParams
{
    public string? ProjectId;
    public string? Status;
    public int Page;
    public int Limit;
}
