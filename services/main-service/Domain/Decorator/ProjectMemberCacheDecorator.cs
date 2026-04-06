using MainService.Domain.Common;
using MainService.Domain.Entities;
using MainService.Domain.Enums;
using MainService.Domain.Interfaces;

namespace MainService.Domain.Decorator;

public class ProjectMemberCacheDecorator : IProjectMemberRepository
{
    private readonly IProjectMemberRepository _innerRepository;
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<ProjectMemberCacheDecorator> _logger;

    private const int CacheExpirationMinutes = 10;

    // Cache metadata for list results
    private class CachedMemberList
    {
        public List<string> MemberIds { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public ProjectMemberCacheDecorator(
        IProjectMemberRepository innerRepository,
        ICacheRepository cacheRepository,
        ILogger<ProjectMemberCacheDecorator> logger)
    {
        _innerRepository = innerRepository;
        _cacheRepository = cacheRepository;
        _logger = logger;
    }

    #region Query Operations

    public async Task<ProjectMemberDomain?> GetAsync(string id)
    {
        var cacheKey = CacheKeys.ProjectMembers.Detail(id);

        try
        {
            var cachedMember = await _cacheRepository.GetAsync<ProjectMemberDomain>(cacheKey);
            if (cachedMember != null)
            {
                _logger.LogDebug("Cache hit: {CacheKey}", cacheKey);
                return cachedMember;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading cache: {CacheKey}", cacheKey);
        }

        // Cache miss → DB
        var member = await _innerRepository.GetAsync(id);
        
        if (member != null)
        {
            await UpsertMemberDetailCacheAsync(member);
        }

        return member;
    }

    public async Task<ProjectMemberDomain?> GetByProjectAndUserAsync(string projectId, string userId)
    {
        var cacheKey = CacheKeys.ProjectMembers.ByProjectAndUser(projectId, userId);

        try
        {
            var cachedMember = await _cacheRepository.GetAsync<ProjectMemberDomain>(cacheKey);
            if (cachedMember != null)
            {
                _logger.LogDebug("Cache hit: {CacheKey}", cacheKey);
                return cachedMember;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading cache: {CacheKey}", cacheKey);
        }

        // Cache miss → DB
        var member = await _innerRepository.GetByProjectAndUserAsync(projectId, userId);
        
        if (member != null)
        {
            await UpsertMemberByProjectAndUserCacheAsync(member);
        }

        return member;
    }

    public async Task<IEnumerable<ProjectMemberDomain>> GetProjectMembersAsync(string projectId, int page, int limit)
    {
        var listCacheKey = CacheKeys.ProjectMembers.ProjectMembersList(projectId, page, limit);

        try
        {
            var cachedList = await _cacheRepository.GetAsync<CachedMemberList>(listCacheKey);

            if (cachedList?.MemberIds?.Any() == true)
            {
                var members = new List<ProjectMemberDomain>();

                foreach (var memberId in cachedList.MemberIds)
                {
                    var detailKey = CacheKeys.ProjectMembers.Detail(memberId);
                    var cachedMember = await _cacheRepository.GetAsync<ProjectMemberDomain>(detailKey);

                    // Cache list exists but detail missing → fallback DB
                    if (cachedMember == null)
                    {
                        _logger.LogWarning(
                            "Cache inconsistency detected. Missing member detail: {MemberId}",
                            memberId);

                        await _cacheRepository.RemoveAsync(listCacheKey);
                        return await _innerRepository.GetProjectMembersAsync(projectId, page, limit);
                    }

                    members.Add(cachedMember);
                }

                _logger.LogDebug("Cache hit: {CacheKey}", listCacheKey);
                return members;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading cache: {CacheKey}", listCacheKey);
        }

        // Cache miss → DB
        var dbMembers = await _innerRepository.GetProjectMembersAsync(projectId, page, limit);

        try
        {
            // Cache member details
            foreach (var member in dbMembers)
            {
                await UpsertMemberDetailCacheAsync(member);
            }

            var cachedList = new CachedMemberList
            {
                MemberIds = dbMembers.Where(m => m.Id != null).Select(m => m.Id!).ToList(),
                TotalCount = dbMembers.Count()
            };

            await _cacheRepository.SetAsync(
                listCacheKey,
                cachedList,
                TimeSpan.FromMinutes(CacheExpirationMinutes));

            _logger.LogDebug("Cache populated: {CacheKey}", listCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing cache: {CacheKey}", listCacheKey);
        }

        return dbMembers;
    }

    public async Task<int> GetProjectMembersCountAsync(string projectId)
    {
        // Count queries are typically fast; optionally cache if needed
        return await _innerRepository.GetProjectMembersCountAsync(projectId);
    }

    public async Task<(IEnumerable<ProjectMemberDomain> Members, int TotalCount)> SearchProjectMembersAsync(SearchProjectMemberQueryParams param)
    {
        var listCacheKey = CacheKeys.ProjectMembers.SearchList(param);

        try
        {
            var cachedList = await _cacheRepository.GetAsync<CachedMemberList>(listCacheKey);

            if (cachedList?.MemberIds?.Any() == true)
            {
                var members = new List<ProjectMemberDomain>();

                foreach (var memberId in cachedList.MemberIds)
                {
                    var detailKey = CacheKeys.ProjectMembers.Detail(memberId);
                    var cachedMember = await _cacheRepository.GetAsync<ProjectMemberDomain>(detailKey);

                    // Cache list exists but detail missing → fallback DB
                    if (cachedMember == null)
                    {
                        _logger.LogWarning(
                            "Cache inconsistency detected. Missing member detail: {MemberId}",
                            memberId);

                        await _cacheRepository.RemoveAsync(listCacheKey);
                        return await _innerRepository.SearchProjectMembersAsync(param);
                    }

                    members.Add(cachedMember);
                }

                _logger.LogDebug("Cache hit: {CacheKey}", listCacheKey);
                return (members, cachedList.TotalCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading cache: {CacheKey}", listCacheKey);
        }

        // Cache miss → DB
        var (dbMembers, totalCount) = await _innerRepository.SearchProjectMembersAsync(param);

        try
        {
            // Cache member details
            foreach (var member in dbMembers)
            {
                await UpsertMemberDetailCacheAsync(member);
            }

            var cachedList = new CachedMemberList
            {
                MemberIds = dbMembers.Where(m => m.Id != null).Select(m => m.Id!).ToList(),
                TotalCount = totalCount
            };

            await _cacheRepository.SetAsync(
                listCacheKey,
                cachedList,
                TimeSpan.FromMinutes(CacheExpirationMinutes));

            _logger.LogDebug("Cache populated: {CacheKey}", listCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing cache: {CacheKey}", listCacheKey);
        }

        return (dbMembers, totalCount);
    }

    public async Task<bool> IsUserProjectMemberAsync(string projectId, string userId)
    {
        // Try to get from cache first
        var member = await GetByProjectAndUserAsync(projectId, userId);
        return member != null;
    }

    public async Task<bool> HasProjectRole(string projectId, string userId, TeamMemberRole role)
    {
        // Try to get from cache first
        var member = await GetByProjectAndUserAsync(projectId, userId);
        return member?.Role == role;
    }

    public async Task<IEnumerable<ProjectMemberDomain>> GetUserProjectsAsync(string userId, int page, int limit)
    {
        var listCacheKey = CacheKeys.ProjectMembers.UserProjectsList(userId, page, limit);

        try
        {
            var cachedList = await _cacheRepository.GetAsync<CachedMemberList>(listCacheKey);

            if (cachedList?.MemberIds?.Any() == true)
            {
                var members = new List<ProjectMemberDomain>();

                foreach (var memberId in cachedList.MemberIds)
                {
                    var detailKey = CacheKeys.ProjectMembers.Detail(memberId);
                    var cachedMember = await _cacheRepository.GetAsync<ProjectMemberDomain>(detailKey);

                    // Cache list exists but detail missing → fallback DB
                    if (cachedMember == null)
                    {
                        _logger.LogWarning(
                            "Cache inconsistency detected. Missing member detail: {MemberId}",
                            memberId);

                        await _cacheRepository.RemoveAsync(listCacheKey);
                        return await _innerRepository.GetUserProjectsAsync(userId, page, limit);
                    }

                    members.Add(cachedMember);
                }

                _logger.LogDebug("Cache hit: {CacheKey}", listCacheKey);
                return members;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading cache: {CacheKey}", listCacheKey);
        }

        // Cache miss → DB
        var dbMembers = await _innerRepository.GetUserProjectsAsync(userId, page, limit);

        try
        {
            // Cache member details
            foreach (var member in dbMembers)
            {
                await UpsertMemberDetailCacheAsync(member);
            }

            var cachedList = new CachedMemberList
            {
                MemberIds = dbMembers.Where(m => m.Id != null).Select(m => m.Id!).ToList(),
                TotalCount = dbMembers.Count()
            };

            await _cacheRepository.SetAsync(
                listCacheKey,
                cachedList,
                TimeSpan.FromMinutes(CacheExpirationMinutes));

            _logger.LogDebug("Cache populated: {CacheKey}", listCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing cache: {CacheKey}", listCacheKey);
        }

        return dbMembers;
    }

    public async Task<int> GetUserProjectsCountAsync(string userId)
    {
        // Count queries are typically fast; optionally cache if needed
        return await _innerRepository.GetUserProjectsCountAsync(userId);
    }

    #endregion

    #region Command Operations

    public async Task<ProjectMemberDomain> AddAsync(ProjectMemberDomain member)
    {
        var addedMember = await _innerRepository.AddAsync(member);

        await UpsertMemberDetailCacheAsync(addedMember);
        await UpsertMemberByProjectAndUserCacheAsync(addedMember);

        // Invalidate related list caches
        await InvalidateProjectMemberListsAsync(addedMember.ProjectId);
        await InvalidateUserProjectListsAsync(addedMember.UserId);

        return addedMember;
    }

    public async Task<ProjectMemberDomain> UpdateAsync(ProjectMemberDomain member)
    {
        var updatedMember = await _innerRepository.UpdateAsync(member);

        await UpsertMemberDetailCacheAsync(updatedMember);
        await UpsertMemberByProjectAndUserCacheAsync(updatedMember);

        // Invalidate related list caches
        await InvalidateProjectMemberListsAsync(updatedMember.ProjectId);
        await InvalidateUserProjectListsAsync(updatedMember.UserId);

        return updatedMember;
    }

    public async Task DeleteAsync(string id)
    {
        // Get member before deletion to know which caches to invalidate
        var member = await _innerRepository.GetAsync(id);

        await _innerRepository.DeleteAsync(id);

        await RemoveMemberDetailCacheAsync(id);

        if (member != null)
        {
            await RemoveMemberByProjectAndUserCacheAsync(member.ProjectId, member.UserId);
            await InvalidateProjectMemberListsAsync(member.ProjectId);
            await InvalidateUserProjectListsAsync(member.UserId);
        }
    }

    public async Task<ProjectMemberDomain> ApproveMemberAsync(string projectId, string userId)
    {
        var approvedMember = await _innerRepository.ApproveMemberAsync(projectId, userId);

        await UpsertMemberDetailCacheAsync(approvedMember);
        await UpsertMemberByProjectAndUserCacheAsync(approvedMember);

        // Invalidate related list caches
        await InvalidateProjectMemberListsAsync(projectId);
        await InvalidateUserProjectListsAsync(userId);

        return approvedMember;
    }

    public async Task<bool> RejectMemberAsync(string projectId, string userId)
    {
        // Get member before rejection to know which caches to invalidate
        var member = await _innerRepository.GetByProjectAndUserAsync(projectId, userId);

        var result = await _innerRepository.RejectMemberAsync(projectId, userId);

        if (result && member != null && !string.IsNullOrEmpty(member.Id))
        {
            await RemoveMemberDetailCacheAsync(member.Id);
            await RemoveMemberByProjectAndUserCacheAsync(projectId, userId);
            await InvalidateProjectMemberListsAsync(projectId);
            await InvalidateUserProjectListsAsync(userId);
        }

        return result;
    }

    public async Task<bool> AddMembersToTeamAsync(string projectId, string teamId, List<string> userIds)
    {
        var result = await _innerRepository.AddMembersToTeamAsync(projectId, teamId, userIds);

        if (result)
        {
            // Invalidate caches for all affected users
            foreach (var userId in userIds)
            {
                var member = await _innerRepository.GetByProjectAndUserAsync(projectId, userId);
                if (member != null)
                {
                    await UpsertMemberDetailCacheAsync(member);
                    await UpsertMemberByProjectAndUserCacheAsync(member);
                }
            }

            await InvalidateProjectMemberListsAsync(projectId);
        }

        return result;
    }

    #endregion

    #region Cache Helpers (Single Responsibility)

    /// <summary>
    /// Create or overwrite member detail cache
    /// </summary>
    private async Task UpsertMemberDetailCacheAsync(ProjectMemberDomain member)
    {
        if (string.IsNullOrEmpty(member.Id))
            return;

        var cacheKey = CacheKeys.ProjectMembers.Detail(member.Id);

        try
        {
            await _cacheRepository.SetAsync(
                cacheKey,
                member,
                TimeSpan.FromMinutes(CacheExpirationMinutes));

            _logger.LogDebug("Upserted member cache: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting member cache: {MemberId}", member.Id);
        }
    }

    /// <summary>
    /// Create or overwrite member cache by project and user
    /// </summary>
    private async Task UpsertMemberByProjectAndUserCacheAsync(ProjectMemberDomain member)
    {
        var cacheKey = CacheKeys.ProjectMembers.ByProjectAndUser(member.ProjectId, member.UserId);

        try
        {
            await _cacheRepository.SetAsync(
                cacheKey,
                member,
                TimeSpan.FromMinutes(CacheExpirationMinutes));

            _logger.LogDebug("Upserted member by project-user cache: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting member by project-user cache: {ProjectId}/{UserId}", 
                member.ProjectId, member.UserId);
        }
    }

    /// <summary>
    /// Remove member detail cache
    /// </summary>
    private async Task RemoveMemberDetailCacheAsync(string memberId)
    {
        var cacheKey = CacheKeys.ProjectMembers.Detail(memberId);

        try
        {
            await _cacheRepository.RemoveAsync(cacheKey);
            _logger.LogDebug("Removed member cache: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member cache: {MemberId}", memberId);
        }
    }

    /// <summary>
    /// Remove member cache by project and user
    /// </summary>
    private async Task RemoveMemberByProjectAndUserCacheAsync(string projectId, string userId)
    {
        var cacheKey = CacheKeys.ProjectMembers.ByProjectAndUser(projectId, userId);

        try
        {
            await _cacheRepository.RemoveAsync(cacheKey);
            _logger.LogDebug("Removed member by project-user cache: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member by project-user cache: {ProjectId}/{UserId}", 
                projectId, userId);
        }
    }

    /// <summary>
    /// Invalidate all member list caches for a project
    /// </summary>
    private async Task InvalidateProjectMemberListsAsync(string projectId)
    {
        var pattern = $"project_members:*:pid:{projectId}:*";

        try
        {
            await _cacheRepository.RemoveByPatternAsync(pattern);
            _logger.LogInformation("Invalidated member list caches for project: {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating member list caches: {ProjectId}", projectId);
        }
    }

    /// <summary>
    /// Invalidate all project list caches for a user
    /// </summary>
    private async Task InvalidateUserProjectListsAsync(string userId)
    {
        var pattern = $"project_members:user_projects:uid:{userId}:*";

        try
        {
            await _cacheRepository.RemoveByPatternAsync(pattern);
            _logger.LogInformation("Invalidated user project list caches for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating user project list caches: {UserId}", userId);
        }
    }

    #endregion
}

