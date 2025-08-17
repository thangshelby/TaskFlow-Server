using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MainService.Domain.UseCases;

public class OtpTokenUseCase
{
    private readonly IOtpTokenRepository _otpTokenRepository;
    private readonly ILogger<OtpTokenUseCase> _logger;
    private readonly IPublisherService _publisher;

    public OtpTokenUseCase(
        IOtpTokenRepository otpTokenRepository,
        ILogger<OtpTokenUseCase> logger,
        IPublisherService publisher)
    {
        _otpTokenRepository = otpTokenRepository;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<OtpTokenDomain> GenerateOtpAsync(UserDomain user, int otpLength = 6, int expireMinutes = 5)
    {
        // 1. Generate OTP code
        var otp = GenerateNumericOtp(otpLength);

        _logger.LogInformation("Generated OTP for user {Email}: {Otp}", user.Email, otp);

        // 2. Hash OTP (to store in DB)
        var salt = Guid.NewGuid().ToString("N");
        var otpHash = HashOtp(otp, salt);

        var otpToken = new OtpTokenDomain
        {
            UserId = user.Id!,
            OtpHash = otpHash,
            Salt = salt,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes),
            MaxAttempts = 5,
            AttemptCount = 0,
            ResendCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var savedToken = await _otpTokenRepository.CreateAsync(otpToken);

        await _publisher.EmitKafka(
            TopicName.MAILS,
            KafkaMessageAction.MAILS_SEND_VERIFY_OTP_USER,
            new IMailMessage
            {
                UserId = user.Id!,
                Data = new
                {
                    OTP = otp,
                    FullName = $"{user.FirstName} {user.LastName}"
                }
            }
        );

        _logger.LogInformation("Generated OTP for user {UserId}", user.Id);
        return savedToken;
    }

    public async Task<bool> VerifyOtpAsync(string userId, string otp)
    {
        var token = await _otpTokenRepository.GetByUserIdAsync(userId);

        if (token == null || token.ExpiresAt < DateTime.UtcNow)
        {
            return false; // OTP hết hạn hoặc không tồn tại
        }

        var otpHash = HashOtp(otp, token.Salt);

        if (token.OtpHash != otpHash)
        {
            await _otpTokenRepository.UpdateAsync(new OtpTokenUpdateParams
            {
                Id = token.Id,
                AttemptCount = token.AttemptCount + 1
            });
            return false; // Sai OTP
        }

        return true;
    }

    private static string GenerateNumericOtp(int length)
    {
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);

        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append((b % 10).ToString());
        }

        return sb.ToString()[..length];
    }

    private static string HashOtp(string otp, string salt)
    {
        var raw = Encoding.UTF8.GetBytes(otp + salt);
        var hash = SHA256.HashData(raw);
        return Convert.ToBase64String(hash);
    }
}
