namespace MainService.Domain.Interfaces;

using MainService.Domain.Entities;

public interface IOtpTokenRepository
{
    Task<OtpTokenDomain> CreateAsync(OtpTokenDomain token);
    Task<OtpTokenDomain?> GetByUserIdAsync(string userId);
    Task UpdateAsync(OtpTokenUpdateParams param);
}
public class OtpTokenUpdateParams
{
    public required string Id { get; set; }
    public int AttemptCount { get; set; }
    public int ResendCount { get; set; }
    public DateTime? CanResendAfter { get; set; }
}