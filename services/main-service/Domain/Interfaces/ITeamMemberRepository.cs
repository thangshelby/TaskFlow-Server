using MainService.Domain.Entities;
using MainService.Domain.Enums;

namespace MainService.Domain.Interfaces;

public interface ITeamMemberRepository
{
    Task<TeamMemberDomain?> GetAsync(string id);
    Task<TeamMemberDomain?> GetByProjectAndUserAsync(string projectId, string userId);
    Task<IEnumerable<TeamMemberDomain>> GetProjectMembersAsync(string projectId, int page, int limit);
    Task<int> GetProjectMembersCountAsync(string projectId);
    Task<TeamMemberDomain> AddAsync(TeamMemberDomain member);
    Task<TeamMemberDomain> UpdateAsync(TeamMemberDomain member);
    Task DeleteAsync(string id);
    Task<bool> IsUserProjectMemberAsync(string projectId, string userId);
    Task<bool> HasProjectRole(string projectId, string userId, TeamMemberRole role);
    Task<IEnumerable<TeamMemberDomain>> GetUserTeamsAsync(string userId, int page, int limit);
    Task<int> GetUserTeamsCountAsync(string userId);
}