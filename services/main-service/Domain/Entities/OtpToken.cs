
namespace MainService.Domain.Entities;

public class OtpTokenDomain
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public string OtpHash { get; set; } = default!;
    public string Salt { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; } = 0;
    public int MaxAttempts { get; set; } = 5;
    public int ResendCount { get; set; } = 0;
    public DateTime ResendWindowStart { get; set; } = DateTime.UtcNow;
    public DateTime CanResendAfter { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum OtpVerifyResult
{
    Success,
    Invalid,
    Expired,
    Locked,
    NotFound
}