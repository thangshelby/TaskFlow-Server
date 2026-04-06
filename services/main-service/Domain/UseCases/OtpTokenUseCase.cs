using Grpc.Core;
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

        await _publisher.EmitQueue(
            QueueTopicName.MAILS,
            QueueMessageAction.MAILS_SEND_VERIFY_OTP_USER,
            new IMailMessage
            {
                UserId = user.Id!,
                Data = new
                {
                    OTP = otp,
                }
            }
        );

        return savedToken;
    }

    public async Task<bool> ResendOtpAsync(string userId)
    {
        var token = await GetTokenOtp(userId);
        if (token == null)
            throw new RpcException(new Status(StatusCode.NotFound, "OTP token not found"));

        if (token.ResendCount >= token.MaxAttempts)
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Max resend attempts reached"));

        if (DateTime.UtcNow < token.CanResendAfter)
            throw new RpcException(new Status(StatusCode.ResourceExhausted, "Please wait before resending OTP"));

        var newOtp = GenerateNumericOtp(6);
        var salt = Guid.NewGuid().ToString("N");
        token.OtpHash = HashOtp(newOtp, salt);

        var updatedToken = await _otpTokenRepository.UpdateAsync(new OtpTokenUpdateParams
        {
            Id = token.Id!,
            Salt = salt,
            OtpHash = token.OtpHash,
            CanResendAfter = DateTime.UtcNow.AddMinutes(1),
            ResendCount = token.ResendCount + 1,
        });

        await _publisher.EmitQueue(
            QueueTopicName.MAILS,
            QueueMessageAction.MAILS_SEND_VERIFY_OTP_USER,
            new IMailMessage
            {
                UserId = userId,
                Data = new
                {
                    OTP = newOtp,
                }
            }
        );

        return true;
    }

    public async Task<OtpTokenDomain?> GetTokenOtp(string userId)
    {
        return await _otpTokenRepository.GetByUserIdAsync(userId);
    }

    public async Task<OtpVerifyResult> VerifyOtpAsync(UserDomain user, string otp)
    {
        if (user.Id == null) return OtpVerifyResult.NotFound;
        var token = await _otpTokenRepository.GetByUserIdAsync(user.Id);

        if (token == null || token.Id == null) return OtpVerifyResult.NotFound;
        if (token.ExpiresAt < DateTime.UtcNow) return OtpVerifyResult.Expired;
        if (token.AttemptCount >= token.MaxAttempts) return OtpVerifyResult.Locked;

        var otpHash = HashOtp(otp, token.Salt);

        if (token.OtpHash != otpHash)
        {
            await _otpTokenRepository.UpdateAsync(new OtpTokenUpdateParams
            {
                Id = token.Id,
                AttemptCount = token.AttemptCount + 1,
            });
            return OtpVerifyResult.Invalid;
        }

        await _otpTokenRepository.MarkUsedAsync(token.Id);

        return OtpVerifyResult.Success;
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
