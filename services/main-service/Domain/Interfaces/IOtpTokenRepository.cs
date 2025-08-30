namespace MainService.Domain.Interfaces;

using MainService.Domain.Entities;

public interface IOtpTokenRepository
{
    Task<OtpTokenDomain> CreateAsync(OtpTokenDomain token);
    Task<OtpTokenDomain?> GetByUserIdAsync(string userId);
    Task<OtpTokenDomain> UpdateAsync(OtpTokenUpdateParams param);

    Task MarkUsedAsync(string tokenId);
}
public class OtpTokenUpdateParams
{
    public required string Id { get; set; }
    public string? OtpHash { get; set; }
    public string? Salt { get; set; }
    public int AttemptCount { get; set; }
    public int ResendCount { get; set; }
    public DateTime? CanResendAfter { get; set; }
}