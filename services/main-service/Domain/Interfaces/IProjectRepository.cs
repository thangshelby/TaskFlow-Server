using MainService.Domain.Entities;
namespace MainService.Domain.Interfaces;
public interface IProjectRepository
{
    Task<ProjectDomain> CreateProject(ProjectDomain project);
    Task<ProjectDomain> GetProject(string id);
    Task<ProjectDomain> UpdateProject(ProjectDomain project);
    Task DeleteProject(string id);
    Task<(List<ProjectDomain> Projects, int TotalCount)> ListProjects(ListProjectParams param);
}
public class ListProjectParams
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string? UserId { get; set; } // optional
}