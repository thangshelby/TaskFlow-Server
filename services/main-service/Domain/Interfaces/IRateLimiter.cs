namespace MainService.Domain.Interfaces;

public interface IRateLimiter
{
    Task<bool> AllowAsync(string key);
}
public class RateLimitOptions
{
    public int Capacity { get; set; } = 50;
    public int RefillPerSecond { get; set; } = 10;
}
