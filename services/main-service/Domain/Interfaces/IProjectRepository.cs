using MainService.Domain.Entities;
namespace MainService.Domain.Interfaces;
public interface IProjectRepository
{
    Task<ProjectDomain> CreateProject(ProjectDomain project);
    Task<ProjectDomain> GetProject(string id);
    Task<ProjectDomain> UpdateProject(ProjectDomain project);
    Task DeleteProject(string id);
    Task<(List<ProjectDomain> Projects, int TotalCount)> ListProjects(ListProjectParams param);
    Task<ProjectColumnDomain> CreateColumn(ProjectColumnDomain projectColumn);
    Task<ProjectColumnDomain> FindColumn(GetColumnParams param);
    Task<List<ProjectColumnDomain>> FindColumnsByProjectId(string projectId);
    Task UpdateColumnOrder(string projectId, string columnId, int order);
    Task<ProjectColumnDomain> UpdateColumn(UpdateColumnParams param);
    Task DeleteColumn(DeleteColumnParams param);
}
public class ListProjectParams
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string? UserId { get; set; }
    public string? Kw { get; set; }
    public string? Sort { get; set; }
}
public class CreateColumnParams
{
    public required string Name { get; set; }
    public required string ProjectId { get; set; }
}

public class UpdateColumnOrdersParams
{
    public required string ProjectId { get; set; }
    public required List<ColumnOrders> Columns { get; set; }
}
public class ColumnOrders
{
    public required string Id { get; set; }
    public required int Order { get; set; }
}
public class UpdateColumnParams
{
    public required string ColumnId { get; set; }
    public string? RemoveIssueId { get; set; }
    public string? AddIssueId { get; set; }
    public string? Name { get; set; }
}
public class GetColumnParams
{
    public string? ColumnId { get; set; }
    public string? Name { get; set; }
}
public class DeleteColumnParams
{
    public required string ColumnId { get; set; }
}