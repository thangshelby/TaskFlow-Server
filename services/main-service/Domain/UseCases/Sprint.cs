using MainService.Domain.Entities;
using MainService.Domain.Interfaces;

namespace MainService.Domain.UseCases;

public class SprintUseCase
{
    private readonly ISprintRepository _sprintRepository;

    public SprintUseCase(ISprintRepository sprintRepository)
    {
        _sprintRepository = sprintRepository;
    }

    public async Task<SprintDomain> CreateSprint(SprintDomain sprint)
    {
        // Add any business logic/validation here
        if (string.IsNullOrEmpty(sprint.Name))
            throw new ArgumentException("Sprint name cannot be empty");

        if (string.IsNullOrEmpty(sprint.ProjectId))
            throw new ArgumentException("Project ID cannot be empty");

        if (sprint.DateStarted >= sprint.DateEnded)
            throw new ArgumentException("Start date must be before end date");

        return await _sprintRepository.CreateSprint(sprint);
    }

    public async Task<SprintDomain> GetSprint(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Sprint ID cannot be empty");

        var sprint = await _sprintRepository.GetSprint(id);
        return sprint ?? throw new KeyNotFoundException($"Sprint with ID {id} not found");
    }

    public async Task<SprintDomain> UpdateSprint(SprintDomain sprint)
    {
        if (string.IsNullOrEmpty(sprint.Id))
            throw new ArgumentException("Sprint ID cannot be empty");

        if (string.IsNullOrEmpty(sprint.Name))
            throw new ArgumentException("Sprint name cannot be empty");

        var existingSprint = await _sprintRepository.GetSprint(sprint.Id);
        if (existingSprint == null)
            throw new KeyNotFoundException($"Sprint with ID {sprint.Id} not found");

        // Update only provided fields
        existingSprint.Name = sprint.Name;
        existingSprint.DateStarted = sprint.DateStarted;
        existingSprint.DateEnded = sprint.DateEnded;
        existingSprint.Duration = sprint.Duration;
        existingSprint.Goal = sprint.Goal;
        existingSprint.ProjectId = sprint.ProjectId;
        existingSprint.UpdatedAt = DateTime.UtcNow;

        return await _sprintRepository.UpdateSprint(existingSprint);
    }

    public async Task DeleteSprint(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Sprint ID cannot be empty");

        var sprint = await _sprintRepository.GetSprint(id);
        if (sprint == null)
            throw new KeyNotFoundException($"Sprint with ID {id} not found");

        await _sprintRepository.DeleteSprint(id);
    }

    public async Task<(List<SprintDomain> Sprints, int TotalCount)> ListSprints(string projectId, int page, int pageSize)
    {
        if (string.IsNullOrEmpty(projectId))
            throw new ArgumentException("Project ID cannot be empty");

        if (page < 1)
            throw new ArgumentException("Page number must be greater than 0");
        
        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0");

        return await _sprintRepository.ListSprints(projectId, page, pageSize);
    }
}