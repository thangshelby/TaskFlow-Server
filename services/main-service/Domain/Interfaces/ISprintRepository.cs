using MainService.Domain.Entities;
namespace MainService.Domain.Interfaces;
public interface ISprintRepository
    {
        Task<SprintDomain> CreateSprint(SprintDomain sprint);
        Task<SprintDomain> GetSprint(string id);
        Task<SprintDomain> UpdateSprint(SprintDomain sprint);
        Task DeleteSprint(string id);
        Task<(List<SprintDomain> Sprints, int TotalCount)> ListSprints(string projectId, int page, int pageSize);
    }