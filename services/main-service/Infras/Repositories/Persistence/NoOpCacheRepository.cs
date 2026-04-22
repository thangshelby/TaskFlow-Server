using MainService.Domain.Interfaces;

namespace MainService.Infras.Repositories;

/// <summary>
/// A no-op implementation of ICacheRepository used when Redis is not configured.
/// All operations gracefully return default values without any actual caching.
/// </summary>
public class NoOpCacheRepository : ICacheRepository
{
    private readonly ILogger<NoOpCacheRepository> _logger;

    public NoOpCacheRepository(ILogger<NoOpCacheRepository> logger)
    {
        _logger = logger;
        _logger.LogInformation("Redis is not configured. Cache operations will be skipped (NoOp mode).");
    }

    public Task<T?> GetAsync<T>(string key) => Task.FromResult(default(T));

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) => Task.CompletedTask;

    public Task RemoveAsync(string key) => Task.CompletedTask;

    public Task<bool> ExistsAsync(string key) => Task.FromResult(false);

    public Task RemoveByPatternAsync(string pattern) => Task.CompletedTask;
}
