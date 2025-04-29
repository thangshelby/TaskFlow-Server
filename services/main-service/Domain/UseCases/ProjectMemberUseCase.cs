using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Enums;


namespace MainService.Domain.UseCases;

public class ProjectMemberUseCase
{
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;

    public ProjectMemberUseCase(IProjectMemberRepository projectMemberRepository, IProjectRepository projectRepository)
    {
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
    }

    public async Task<ProjectMemberDomain> AddProjectMemberAsync(string projectId, string requesterId, string userId, TeamMemberRole role)
    {
        var project = await _projectRepository.GetProject(projectId) 
            ?? throw new KeyNotFoundException("Project not found");

        var existingMember = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, userId);
        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already a member of this project");
        }

        // Check if requester is project owner
        var requester = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, requesterId);
        var isPending = requester?.Role != TeamMemberRole.Owner;

        var member = new ProjectMemberDomain
        {
            ProjectId = projectId,
            UserId = userId,
            Role = role,
            IsPending = isPending
        };

        return await _projectMemberRepository.AddAsync(member);
    }

    public async Task<ProjectMemberDomain> UpdateProjectMemberRoleAsync(string projectId, string userId, TeamMemberRole newRole)
    {
        var member = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, userId)
            ?? throw new KeyNotFoundException("Project member not found");

        if (member.Role == TeamMemberRole.Owner && newRole != TeamMemberRole.Owner)
        {
            throw new InvalidOperationException("Cannot change the role of project owner");
        }

        member.Role = newRole;
        member.UpdatedAt = DateTime.UtcNow;

        return await _projectMemberRepository.UpdateAsync(member);
    }

    public async Task RemoveProjectMemberAsync(string projectId, string userId)
    {
        var member = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, userId)
            ?? throw new KeyNotFoundException("Project member not found");

        if (member.Role == TeamMemberRole.Owner)
        {
            throw new InvalidOperationException("Cannot remove project owner from project");
        }

        await _projectMemberRepository.DeleteAsync(member.Id!);
    }

    public async Task<(IEnumerable<ProjectMemberDomain> Members, int TotalCount)> GetProjectMembersAsync(
        string projectId, int page, int limit)
    {
        var members = await _projectMemberRepository.GetProjectMembersAsync(projectId, page, limit);
        var totalCount = await _projectMemberRepository.GetProjectMembersCountAsync(projectId);

        return (members, totalCount);
    }

    public async Task<bool> IsUserProjectMemberAsync(string projectId, string userId)
    {
        return await _projectMemberRepository.IsUserProjectMemberAsync(projectId, userId);
    }

    public async Task<bool> HasProjectRoleAsync(string projectId, string userId, TeamMemberRole role)
    {
        return await _projectMemberRepository.HasProjectRole(projectId, userId, role);
    }

    public async Task<(IEnumerable<ProjectMemberDomain> Projects, int TotalCount)> GetUserProjectsAsync(
        string userId, int page, int limit)
    {
        var projects = await _projectMemberRepository.GetUserProjectsAsync(userId, page, limit);
        var totalCount = await _projectMemberRepository.GetUserProjectsCountAsync(userId);

        return (projects, totalCount);
    }

    public async Task<ProjectMemberDomain> ApproveProjectMemberAsync(string projectId, string approverId, string userId)
    {
        // Check if approver is project owner
        var approver = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, approverId)
            ?? throw new KeyNotFoundException("Approver not found in project");

        if (approver.Role != TeamMemberRole.Owner)
        {
            throw new UnauthorizedAccessException("Only project owner can approve members");
        }

        var member = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, userId)
            ?? throw new KeyNotFoundException("Project member not found");

        if (!member.IsPending)
        {
            throw new InvalidOperationException("Member is already approved");
        }

        return await _projectMemberRepository.ApproveMemberAsync(projectId, userId);
    }

    public async Task RejectProjectMemberAsync(string projectId, string approverId, string userId)
    {
        // Check if approver is project owner
        var approver = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, approverId)
            ?? throw new KeyNotFoundException("Approver not found in project");

        if (approver.Role != TeamMemberRole.Owner)
        {
            throw new UnauthorizedAccessException("Only project owner can reject members");
        }

        var result = await _projectMemberRepository.RejectMemberAsync(projectId, userId);
        if (!result)
        {
            throw new KeyNotFoundException("Pending project member not found");
        }
    }
}