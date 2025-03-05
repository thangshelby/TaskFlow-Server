using MainService.Domain.Entities;
namespace MainService.Domain.Interfaces;
public interface IProjectRepository
{
    Task<ProjectDomain> CreateProject(ProjectDomain user);
}
