using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Enums;

namespace MainService.Domain.UseCases;

public class TeamMemberUseCase
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IProjectRepository _projectRepository;

    public TeamMemberUseCase(ITeamMemberRepository teamMemberRepository, IProjectRepository projectRepository)
    {
        _teamMemberRepository = teamMemberRepository;
        _projectRepository = projectRepository;
    }

    public async Task<TeamMemberDomain> AddTeamMemberAsync(string projectId, string userId, TeamMemberRole role)
    {
        var project = await _projectRepository.GetProject(projectId) 
            ?? throw new KeyNotFoundException("Project not found");

        var existingMember = await _teamMemberRepository.GetByProjectAndUserAsync(projectId, userId);
        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already a member of this project");
        }

        var member = new TeamMemberDomain
        {
            ProjectId = projectId,
            UserId = userId,
            Role = role
        };

        return await _teamMemberRepository.AddAsync(member);
    }

    public async Task<TeamMemberDomain> UpdateTeamMemberRoleAsync(string projectId, string userId, TeamMemberRole newRole)
    {
        var member = await _teamMemberRepository.GetByProjectAndUserAsync(projectId, userId)
            ?? throw new KeyNotFoundException("Team member not found");

        if (member.Role == TeamMemberRole.Owner && newRole != TeamMemberRole.Owner)
        {
            throw new InvalidOperationException("Cannot change the role of project owner");
        }

        member.Role = newRole;
        member.UpdatedAt = DateTime.UtcNow;

        return await _teamMemberRepository.UpdateAsync(member);
    }

    public async Task RemoveTeamMemberAsync(string projectId, string userId)
    {
        var member = await _teamMemberRepository.GetByProjectAndUserAsync(projectId, userId)
            ?? throw new KeyNotFoundException("Team member not found");

        if (member.Role == TeamMemberRole.Owner)
        {
            throw new InvalidOperationException("Cannot remove project owner from team");
        }

        await _teamMemberRepository.DeleteAsync(member.Id!);
    }

    public async Task<(IEnumerable<TeamMemberDomain> Members, int TotalCount)> GetProjectMembersAsync(
        string projectId, int page, int limit)
    {
        var members = await _teamMemberRepository.GetProjectMembersAsync(projectId, page, limit);
        var totalCount = await _teamMemberRepository.GetProjectMembersCountAsync(projectId);

        return (members, totalCount);
    }

    public async Task<bool> IsUserProjectMemberAsync(string projectId, string userId)
    {
        return await _teamMemberRepository.IsUserProjectMemberAsync(projectId, userId);
    }

    public async Task<bool> HasProjectRoleAsync(string projectId, string userId, TeamMemberRole role)
    {
        return await _teamMemberRepository.HasProjectRole(projectId, userId, role);
    }

    public async Task<(IEnumerable<TeamMemberDomain> Teams, int TotalCount)> GetMyTeamsAsync(
        string userId, int page, int limit)
    {
        var teams = await _teamMemberRepository.GetUserTeamsAsync(userId, page, limit);
        var totalCount = await _teamMemberRepository.GetUserTeamsCountAsync(userId);

        return (teams, totalCount);
    }
}