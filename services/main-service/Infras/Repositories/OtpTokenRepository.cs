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

    public async Task<OtpTokenDomain> UpdateAsync(OtpTokenUpdateParams updateParams)
    {
        var update = MongoUtils.MakeMongoDataUpdate<OtpToken>(new MongoUtils.MongoUpdateInput
        {
            Data = updateParams,
        });

        await _otpTokens.UpdateOneAsync(
            x => x.Id == updateParams.Id,
            update
        );

        var updatedToken = await _otpTokens.Find(token => token.Id == updateParams.Id).FirstOrDefaultAsync();
        return _mapper.Map<OtpTokenDomain>(updatedToken);
    }

    public async Task MarkUsedAsync(string tokenId)
    {
        var update = Builders<OtpToken>.Update
            .Set(t => t.ExpiresAt, DateTime.UtcNow)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        await _otpTokens.UpdateOneAsync(t => t.Id == tokenId, update);
        _logger.LogInformation("OTP token {TokenId} marked as used", tokenId);
    }
}
