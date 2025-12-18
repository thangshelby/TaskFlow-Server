using MainService.Domain.Common;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using TaskFlow.UserService;

namespace MainService.Infras.Repositories;

public class IssueRepositoryCacheDecorator : IIssueRepository
{
    private readonly IIssueRepository _innerRepository;
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<IssueRepositoryCacheDecorator> _logger;
    private const int CacheExpirationMinutes = 10;

    public IssueRepositoryCacheDecorator(
        IIssueRepository innerRepository,
        ICacheRepository cacheRepository,
        ILogger<IssueRepositoryCacheDecorator> logger)
    {
        _innerRepository = innerRepository;
        _cacheRepository = cacheRepository;
        _logger = logger;
    }

    private string GetListIssuesCacheKey(GetIssuesParams param)
    {
        var keyParts = new List<string>
        {
            "issues:list",
            $"pid:{param.ProjectId ?? "all"}",
            $"page:{param.Page}",
            $"limit:{param.Limit}",
            $"keyword:{param.Keyword ?? "none"}",
        };

        if (param.ColumnIds != null && param.ColumnIds.Any())
        {
            keyParts.Add($"cols:{string.Join(",", param.ColumnIds.OrderBy(x => x))}");
        }

        if (param.AssigneeIds != null && param.AssigneeIds.Any())
        {
            keyParts.Add($"assignees:{string.Join(",", param.AssigneeIds.OrderBy(x => x))}");
        }

        if (param.SprintIds != null && param.SprintIds.Any())
        {
            keyParts.Add($"sprints:{string.Join(",", param.SprintIds.OrderBy(x => x))}");
        }

        if (param.Types != null && param.Types.Any())
        {
            keyParts.Add($"types:{string.Join(",", param.Types.OrderBy(x => x))}");
        }

        if (param.Priorities != null && param.Priorities.Any())
        {
            keyParts.Add($"priorities:{string.Join(",", param.Priorities.OrderBy(x => x))}");
        }

        if (param.TeamIds != null && param.TeamIds.Any())
        {
            keyParts.Add($"teams:{string.Join(",", param.TeamIds.OrderBy(x => x))}");
        }

        if (param.ParentIds != null && param.ParentIds.Any())
        {
            keyParts.Add($"parents:{string.Join(",", param.ParentIds.OrderBy(x => x))}");
        }

        return string.Join(":", keyParts);
    }

    public async Task<PagedResult<IssueDomain>> ListIssues(GetIssuesParams param)
    {
        var cacheKey = CacheKeys.Issues.List(param);
        
        try
        {
            // Try to get from cache
            var cachedResult = await _cacheRepository.GetAsync<PagedResult<IssueDomain>>(cacheKey);
            if (cachedResult != null && cachedResult.Items != null && cachedResult.Items.Any())
            {
                var result = new PagedResult<IssueDomain>
                _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                cachedResult.
                return cachedResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from cache for key: {CacheKey}", cacheKey);
        }

        // Cache miss - get from database
        _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
        var result = await _innerRepository.ListIssues(param);

        // Store in cache
        try
        {
            var 
            await _cacheRepository.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to cache for key: {CacheKey}", cacheKey);
        }

        return result;
    }

    public async Task<IssueDomain> CreateIssue(IssueDomain issue)
    {
        var result = await _innerRepository.CreateIssue(issue);
        
        // Invalidate list caches for this project
        await InvalidateProjectCaches(issue.ProjectId);
        
        return result;
    }

    public async Task<IssueDomain> GetIssue(string id)
    {
        return await _innerRepository.GetIssue(id);
    }

    public async Task<IssueDomain> UpdateIssue(UpdateIssueParams issue)
    {
        var result = await _innerRepository.UpdateIssue(issue);
        
        // Get the project ID to invalidate caches
        if (!string.IsNullOrEmpty(result.ProjectId))
        {
            await InvalidateProjectCaches(result.ProjectId);
        }
        
        return result;
    }

    public async Task DeleteIssue(string id)
    {
        // Get issue first to know which project caches to invalidate
        var issue = await _innerRepository.GetIssue(id);
        await _innerRepository.DeleteIssue(id);
        
        if (!string.IsNullOrEmpty(issue.ProjectId))
        {
            await InvalidateProjectCaches(issue.ProjectId);
        }
    }

    public async Task<UserStats> GetStats(string id, bool isSprintId)
    {
        return await _innerRepository.GetStats(id, isSprintId);
    }

    private async Task InvalidateProjectCaches(string projectId)
    {
        try
        {
            var pattern = $"issues:list:pid:{projectId}:*";
            await _cacheRepository.RemoveByPatternAsync(pattern);
            _logger.LogInformation("Invalidated caches for project: {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating project caches for: {ProjectId}", projectId);
        }
    }
}

