using MainService.Domain.Entities;
using MainService.Domain.Enums;

namespace MainService.Domain.Interfaces;

public interface IProjectMemberRepository
{
    Task<ProjectMemberDomain?> GetAsync(string id);
    Task<ProjectMemberDomain?> GetByProjectAndUserAsync(string projectId, string userId);
    Task<IEnumerable<ProjectMemberDomain>> GetProjectMembersAsync(string projectId, int page, int limit);
    Task<int> GetProjectMembersCountAsync(string projectId);
    Task<ProjectMemberDomain> AddAsync(ProjectMemberDomain member);
    Task<ProjectMemberDomain> UpdateAsync(ProjectMemberDomain member);
    Task DeleteAsync(string id);
    Task<bool> IsUserProjectMemberAsync(string projectId, string userId);
    Task<bool> HasProjectRole(string projectId, string userId, TeamMemberRole role);
    Task<IEnumerable<ProjectMemberDomain>> GetUserProjectsAsync(string userId, int page, int limit);
    Task<int> GetUserProjectsCountAsync(string userId);
    Task<ProjectMemberDomain> ApproveMemberAsync(string projectId, string userId);
    Task<bool> RejectMemberAsync(string projectId, string userId);
}