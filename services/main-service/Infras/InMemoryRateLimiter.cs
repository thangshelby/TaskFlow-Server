using System.Collections.Concurrent;
using MainService.Domain.Interfaces;

namespace MainService.Infras;

/// <summary>
/// An in-memory token bucket rate limiter used when Redis is not configured.
/// Uses ConcurrentDictionary for thread-safe, per-key rate limiting.
/// </summary>
public class InMemoryRateLimiter : IRateLimiter
{
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

    public InMemoryRateLimiter(RateLimitOptions options)
    {
        _options = options;
    }

    public Task<bool> AllowAsync(string key)
    {
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_options.Capacity, _options.RefillPerSecond));
        return Task.FromResult(bucket.TryConsume());
    }

    private class TokenBucket
    {
        private readonly int _capacity;
        private readonly int _refillPerSecond;
        private double _tokens;
        private long _lastTimestamp;
        private readonly object _lock = new();

        public TokenBucket(int capacity, int refillPerSecond)
        {
            _capacity = capacity;
            _refillPerSecond = refillPerSecond;
            _tokens = capacity;
            _lastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public bool TryConsume()
        {
            lock (_lock)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var delta = Math.Max(0, now - _lastTimestamp);
                _tokens = Math.Min(_capacity, _tokens + delta * _refillPerSecond);
                _lastTimestamp = now;

                if (_tokens < 1)
                    return false;

                _tokens -= 1;
                return true;
            }
        }
    }
}
