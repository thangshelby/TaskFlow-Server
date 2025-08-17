using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras;
using MainService.Infras.Entities;
using MongoDB.Driver;

public class OtpTokenRepository : IOtpTokenRepository
{
    private readonly IMongoCollection<OtpToken> _otpTokens;
    private readonly IMapper _mapper;
    private readonly ILogger<OtpTokenRepository> _logger;

    public OtpTokenRepository(MongoDbService mongoDbService, IMapper mapper, ILogger<OtpTokenRepository> logger)
    {
        var database = mongoDbService.Database;
        _otpTokens = database.GetCollection<OtpToken>("otp_tokens");
        _mapper = mapper;
        _logger = logger;

        // TTL index để auto xoá khi hết hạn
        var ttlKeys = Builders<OtpToken>.IndexKeys.Ascending(t => t.ExpiresAt);
        var ttlOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
        var ttlModel = new CreateIndexModel<OtpToken>(ttlKeys, ttlOptions);
        _otpTokens.Indexes.CreateOne(ttlModel);
    }

    public async Task<OtpTokenDomain> CreateAsync(OtpTokenDomain token)
    {
        var tokenEntity = _mapper.Map<OtpToken>(token);
        await _otpTokens.InsertOneAsync(tokenEntity);
        return token;
    }

    public async Task<OtpTokenDomain?> GetByUserIdAsync(string userId)
    {
        var entity = await _otpTokens.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        return entity == null ? null : _mapper.Map<OtpTokenDomain>(entity);
    }

    public async Task UpdateAsync(OtpTokenUpdateParams updateParams)
    {
        var update = Builders<OtpToken>.Update
            .Set(t => t.AttemptCount, updateParams.AttemptCount)
            .Set(t => t.ResendCount, updateParams.ResendCount)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        if (updateParams.CanResendAfter != null)
        {
            update = update.Set(t => t.CanResendAfter, updateParams.CanResendAfter.Value);
        }

        await _otpTokens.UpdateOneAsync(
            x => x.Id == updateParams.Id,
            update
        );
    }
}
