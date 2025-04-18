using MainService.Domain.Entities;
using MainService.Domain.Interfaces;

namespace MainService.Domain.UseCases;

public class ProjectUseCase
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IProjectRepository _projectRepository;

    public ProjectUseCase(IProjectRepository projectRepository, ITransactionRepo transactionRepo)
    {
        _projectRepository = projectRepository;
        _transactionRepo = transactionRepo;
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
    public async Task<(List<ProjectDomain> Projects, int TotalCount)> ListProjects(ListProjectParams param)
    {
        return await _projectRepository.ListProjects(param);
    }
    public async Task<ProjectColumnDomain> CreateColumn(CreateColumnParams param)
    {
        // TODO : FIX CORCUR
        var existingColumns = await _projectRepository.FindColumnsByProjectId(param.ProjectId);

        var highestOrder = existingColumns.Count != 0 ? existingColumns.Max(c => c.Order) : 0;

        var newOrder = highestOrder + 1;

        var projectColumnDomain = new ProjectColumnDomain
        {
            Name = param.Name,
            ProjectId = param.ProjectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Issues = [],
            Order = newOrder
        };
        return await _projectRepository.CreateColumn(projectColumnDomain);
    }
    public async Task<List<ProjectColumnDomain>> GetAllColumns(string projectId)
    {
        return await _projectRepository.FindColumnsByProjectId(projectId);
    }
    public async Task<List<ProjectColumnDomain>> UpdateColumnsOrder(UpdateColumnOrdersParams param)
    {
        // TODO: validate columns_id belong to project

        var sortedColumns = param.Columns.OrderBy(c => c.Order).ToList();
        await _transactionRepo.ExecuteAsync(async session =>
        {
            foreach (var column in sortedColumns)
            {
                await _projectRepository.UpdateColumnOrder(param.ProjectId, column.Id, column.Order);
            }
        });

        var updatedColumns = await _projectRepository.FindColumnsByProjectId(param.ProjectId);
        return updatedColumns;
    }
    public async Task<ProjectColumnDomain> UpdateColumn(UpdateColumnParams param)
    {
        return await _projectRepository.UpdateColumn(param);
    }
}