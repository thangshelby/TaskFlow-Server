using MainService.Domain.Common;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using TaskFlow.UserService;

namespace MainService.Domain.Decorator;

public class IssueCacheDecorator : IIssueRepository
{
    private readonly IIssueRepository _innerRepository;
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<IssueCacheDecorator> _logger;

    private const int CacheExpirationMinutes = 10;

    // Cache metadata for list result
    private class CachedIssueList
    {
        public List<string> IssueIds { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public IssueCacheDecorator(
        IIssueRepository innerRepository,
        ICacheRepository cacheRepository,
        ILogger<IssueCacheDecorator> logger)
    {
        _innerRepository = innerRepository;
        _cacheRepository = cacheRepository;
        _logger = logger;
    }

    #region Query

    public async Task<PagedResult<IssueDomain>> ListIssues(GetIssuesParams param)
    {
        if (param.Unpaged)
        {
            return await _innerRepository.ListIssues(param);
        }

        var listCacheKey = CacheKeys.Issues.List(param);
        PagedResult<IssueDomain>? dbResult = null;

        try
        {
            var cachedList = await _cacheRepository.GetAsync<CachedIssueList>(listCacheKey);

            // if (cachedList?.IssueIds?.Any() == true)
            // {
            //     var result = new PagedResult<IssueDomain>
            //     {
            //         Items = new List<IssueDomain>(),
            //         TotalCount = cachedList.TotalCount
            //     };

            //     var listFullyResolvedFromCache = true;
            //     foreach (var issueId in cachedList.IssueIds)
            //     {
            //         var detailKey = CacheKeys.Issues.Detail(issueId);
            //         var cachedIssue = await _cacheRepository.GetAsync<IssueDomain>(detailKey);

            //         // Cache list exists but detail missing → remove stale list, load DB, repopulate cache below
            //         if (cachedIssue == null)
            //         {
            //             _logger.LogWarning(
            //                 "Cache inconsistency detected. Missing issue detail: {IssueId}",
            //                 issueId);

            //             await _cacheRepository.RemoveAsync(listCacheKey);
            //             dbResult = await _innerRepository.ListIssues(param);
            //             listFullyResolvedFromCache = false;
            //             break;
            //         }

            //         result.Items.Add(cachedIssue);
            //     }

            //     if (listFullyResolvedFromCache)
            //     {
            //         _logger.LogDebug("Cache hit: {CacheKey}", listCacheKey);
            //         return result;
            //     }
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading cache: {CacheKey}", listCacheKey);
        }

        // Cache miss or inconsistency recovery → DB (if not already loaded)
        dbResult ??= await _innerRepository.ListIssues(param);

        try
        {
            // Cache issue details
            foreach (var issue in dbResult.Items)
            {
                await UpsertIssueDetailCacheAsync(issue);
            }

            var cachedList = new CachedIssueList
            {
                IssueIds = dbResult.Items.Where(i => i.Id != null).Select(i => i.Id!).ToList(),
                TotalCount = dbResult.TotalCount
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

        return dbResult;
    }

    public async Task<IssueDomain> GetIssue(string id)
    {
        // NOTE: có thể cache thêm nếu muốn, hiện giữ nguyên
        return await _innerRepository.GetIssue(id);
    }

    public async Task<UserStats> GetStats(string id, bool isSprintId)
    {
        return await _innerRepository.GetStats(id, isSprintId);
    }

    #endregion

    #region Command

    public async Task<IssueDomain> CreateIssue(IssueDomain issue)
    {
        var createdIssue = await _innerRepository.CreateIssue(issue);

        await UpsertIssueDetailCacheAsync(createdIssue);

        if (!string.IsNullOrEmpty(createdIssue.ProjectId))
        {
            await InvalidateProjectIssueListsAsync(createdIssue.ProjectId);
        }

        return createdIssue;
    }

    public async Task<IssueDomain> UpdateIssue(UpdateIssueParams issue)
    {
        var updatedIssue = await _innerRepository.UpdateIssue(issue);

        // List cache stores page IDs + total; upserting detail refreshes item payloads on list hits, not membership/order/filter.
        await UpsertIssueDetailCacheAsync(updatedIssue);

        return updatedIssue;
    }

    public async Task DeleteIssue(string issueId)
    {
        var issue = await _innerRepository.GetIssue(issueId);

        await _innerRepository.DeleteIssue(issueId);

        await RemoveIssueDetailCacheAsync(issueId);

        if (!string.IsNullOrEmpty(issue.ProjectId))
        {
            await InvalidateProjectIssueListsAsync(issue.ProjectId);
        }
    }

    #endregion

    #region Cache Helpers (Single Responsibility)

    /// <summary>
    /// Create or overwrite issue detail cache
    /// </summary>
    private async Task UpsertIssueDetailCacheAsync(IssueDomain issue)
    {
        if (string.IsNullOrEmpty(issue.Id))
            return;

        var cacheKey = CacheKeys.Issues.Detail(issue.Id);

        try
        {
            await _cacheRepository.SetAsync(
                cacheKey,
                issue,
                TimeSpan.FromMinutes(CacheExpirationMinutes));

            _logger.LogDebug("Upserted issue cache: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting issue cache: {IssueId}", issue.Id);
        }
    }

    /// <summary>
    /// Remove issue detail cache
    /// </summary>
    private async Task RemoveIssueDetailCacheAsync(string issueId)
    {
        var cacheKey = CacheKeys.Issues.Detail(issueId);

        try
        {
            await _cacheRepository.RemoveAsync(cacheKey);
            _logger.LogDebug("Removed issue cache: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing issue cache: {IssueId}", issueId);
        }
    }

    /// <summary>
    /// Invalidate all issue list caches of a project
    /// </summary>
    private async Task InvalidateProjectIssueListsAsync(string projectId)
    {
        var pattern = $"issues:list:pid:{projectId}:*";

        try
        {
            await _cacheRepository.RemoveByPatternAsync(pattern);
            _logger.LogInformation("Invalidated issue list caches for project: {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating issue list caches: {ProjectId}", projectId);
        }
    }
    #endregion
}
