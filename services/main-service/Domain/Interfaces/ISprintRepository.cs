using MainService.Domain.Entities;
using TaskFlow.SprintService;
namespace MainService.Domain.Interfaces;

public interface ISprintRepository
{
    Task<SprintDomain> CreateSprint(SprintDomain sprint);
    Task<SprintDomain> GetSprint(string id);
    Task<SprintDomain> UpdateSprint(SprintDomain sprint);
    Task DeleteSprint(string id);
    Task<SprintStats> GetSprintStats(string sprint_id, string project_id);
    Task<List<SprintDailyStats>> GetSprintDailyStats(string sprint_id);
    Task<(List<SprintDomain> Sprints, int TotalCount)> ListSprints(string projectId, int page, int pageSize);
}