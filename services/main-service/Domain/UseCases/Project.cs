using MainService.Domain.Entities;
using MainService.Domain.Interfaces;

namespace MainService.Domain.UseCases;

public class ProjectUseCase
{
    private readonly IProjectRepository _projectRepository;

    public ProjectUseCase(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDomain> CreateProject(ProjectDomain project)
    {
        // Add any business logic/validation here
        if (string.IsNullOrEmpty(project.Name))
            throw new ArgumentException("Project name cannot be empty");
        
        if (string.IsNullOrEmpty(project.Key))
            throw new ArgumentException("Project key cannot be empty");

        return await _projectRepository.CreateProject(project);
    }

    public async Task<ProjectDomain> GetProject(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Project ID cannot be empty");

        var project = await _projectRepository.GetProject(id);
        return project ?? throw new KeyNotFoundException($"Project with ID {id} not found");
    }

    public async Task<ProjectDomain> UpdateProject(ProjectDomain project)
    {
        if (string.IsNullOrEmpty(project.Id))
            throw new ArgumentException("Project ID cannot be empty");

        if (string.IsNullOrEmpty(project.Name))
            throw new ArgumentException("Project name cannot be empty");

        var existingProject = await _projectRepository.GetProject(project.Id);
        if (existingProject == null)
            throw new KeyNotFoundException($"Project with ID {project.Id} not found");

        // Update only provided fields
        existingProject.Name = project.Name;
        existingProject.Key = project.Key;
        existingProject.Access = project.Access;
        existingProject.Type = project.Type;
        existingProject.OwnerId = project.OwnerId;
        existingProject.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateProject(existingProject);
    }

    public async Task DeleteProject(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Project ID cannot be empty");

        var project = await _projectRepository.GetProject(id);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {id} not found");

        await _projectRepository.DeleteProject(id);
    }

    public async Task<(List<ProjectDomain> Projects, int TotalCount)> ListProjects(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Page number must be greater than 0");
        
        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0");

        return await _projectRepository.ListProjects(page, pageSize);
    }
}