using StackExchange.Redis;
using MainService.Domain.Interfaces;

public class RedisTokenBucketRateLimiter : IRateLimiter
{
    private readonly IDatabase _db;
    private readonly RateLimitOptions _options;

    private const string LuaScript = @"
local key = KEYS[1]
local capacity = tonumber(ARGV[1])
local refill_rate = tonumber(ARGV[2])
local now = tonumber(ARGV[3])

local data = redis.call('HMGET', key, 'tokens', 'timestamp')
local tokens = tonumber(data[1]) or capacity
local last = tonumber(data[2]) or now

local delta = math.max(0, now - last)
tokens = math.min(capacity, tokens + delta * refill_rate)

if tokens < 1 then
  redis.call('HMSET', key, 'tokens', tokens, 'timestamp', now)
  redis.call('EXPIRE', key, 60)
  return 0
end

tokens = tokens - 1
redis.call('HMSET', key, 'tokens', tokens, 'timestamp', now)
redis.call('EXPIRE', key, 60)
return 1
";

    public RedisTokenBucketRateLimiter(
        IConnectionMultiplexer redis,
        RateLimitOptions options)
    {
        _db = redis.GetDatabase();
        _options = options;
    }

    public async Task<bool> AllowAsync(string key)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var result = (int)await _db.ScriptEvaluateAsync(
            LuaScript,
            new RedisKey[] { $"rate_limit:{key}" },
            new RedisValue[]
            {
                _options.Capacity,
                _options.RefillPerSecond,
                now
            });

        return result == 1;
    }
}
