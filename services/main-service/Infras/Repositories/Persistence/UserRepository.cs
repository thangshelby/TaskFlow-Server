using AutoMapper;
using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace MainService.Infras.Repositories;

public static class StringExtensions
{
    public static string NormalizeVietnamese(this string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString();
    }
}

public static class RegexHelper
{
    private static readonly Dictionary<char, string> AccentPatterns = new Dictionary<char, string>
    {
        {'a', "[aáàảãạăắằẳẵặâấầẩẫậ]"},
        {'e', "[eéèẻẽẹêếềểễệ]"},
        {'i', "[iíìỉĩị]"},
        {'o', "[oóòỏõọôốồổỗộơớờởỡợ]"},
        {'u', "[uúùủũụưứừửữự]"},
        {'y', "[yýỳỷỹỵ]"},
        {'d', "[dđ]"}
    };

    public static string BuildAccentInsensitivePattern(string input)
    {
        var result = new StringBuilder();
        foreach (var c in input.ToLower())
        {
            if (AccentPatterns.ContainsKey(c))
                result.Append(AccentPatterns[c]);
            else
                result.Append(Regex.Escape(c.ToString()));
        }
        return result.ToString();
    }
}

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly IMapper _mapper;
    private readonly ILogger<UserRepository> _logger;
    public UserRepository(MongoDbService mongoDbService, IMapper mapper, ILogger<UserRepository> logger)
    {
        var database = mongoDbService.Database;
        _users = database.GetCollection<User>("users");
        _mapper = mapper;
        _logger = logger;


        // Indexing email (unique)
        var emailKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var emailOptions = new CreateIndexOptions { Unique = true };
        var emailModel = new CreateIndexModel<User>(emailKeys, emailOptions);
        _users.Indexes.CreateOne(emailModel);

        // TTL index for ExpireAt
        var ttlKeys = Builders<User>.IndexKeys.Ascending(u => u.ExpiredAt);
        var ttlOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
        var ttlModel = new CreateIndexModel<User>(ttlKeys, ttlOptions);
        _users.Indexes.CreateOne(ttlModel);
    }

    public async Task<UserDomain> CreateUserAsync(UserDomain userDomain)
    {
        // Convert Domain to Entity
        var userEntity = _mapper.Map<User>(userDomain);

        try
        {
            await _users.InsertOneAsync(userEntity);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
        {
            throw new UserAlreadyExistsException("An account with this email already exists.");
        }

        // Update domain with new ID
        userDomain.Id = userEntity.Id;
        return userDomain;
    }

    public async Task<UserDomain?> FindUserAsync(UserQueryParams query)
    {
        var filters = new List<FilterDefinition<User>>();

        if (!string.IsNullOrEmpty(query.UserId))
        {
            filters.Add(Builders<User>.Filter.Eq(x => x.Id, query.UserId));
        }

        if (!string.IsNullOrEmpty(query.Email))
        {
            filters.Add(Builders<User>.Filter.Eq(x => x.Email, query.Email));
        }

        var filter = filters.Count > 0 ? Builders<User>.Filter.And(filters) : Builders<User>.Filter.Empty;

        var userEntity = await _users.Find(filter).FirstOrDefaultAsync();

        return userEntity == null ? null : _mapper.Map<UserDomain>(userEntity);
    }
    public async Task<UserDomain> UpdateUserAsync(UpdateUserParams param)
    {
        var update = MongoUtils.MakeMongoDataUpdate<User>(new MongoUtils.MongoUpdateInput
        {
            Data = param,
        });

        UpdateResult result;
        try
        {
            result = await _users.UpdateOneAsync(
                u => u.Id == param.Id,
                update
            );
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
        {
            throw new UserAlreadyExistsException("An account with this email already exists.");
        }

        if (result.MatchedCount == 0)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        var updatedUser = await _users.Find(u => u.Id == param.Id).FirstOrDefaultAsync();
        return _mapper.Map<UserDomain>(updatedUser);
    }

    public async Task<(IEnumerable<UserDomain> Users, int TotalCount)> SearchUsersAsync(SearchUserQueryParams param)
    {
        var name = param.Name;
        var email = param.Email;
        var page = param.Page ?? 1;
        var limit = param.Limit ?? 10;
        var userIds = param.UserIds;

        var filterBuilder = Builders<User>.Filter;
        var filters = new List<FilterDefinition<User>>();

        if (userIds != null && userIds.Any())
        {
            filters.Add(filterBuilder.In(x => x.Id, userIds));
        }

        if (!string.IsNullOrEmpty(name))
        {
            var decodedName = Uri.UnescapeDataString(name.Replace("+", " "));
            var normalizedName = decodedName.NormalizeVietnamese();
            var searchTerms = normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var nameFilters = new List<FilterDefinition<User>>();
            foreach (var term in searchTerms)
            {
                var pattern = RegexHelper.BuildAccentInsensitivePattern(term);
                nameFilters.Add(filterBuilder.Or(
                    filterBuilder.Regex(x => x.FirstName, new BsonRegularExpression(pattern, "i")),
                    filterBuilder.Regex(x => x.LastName, new BsonRegularExpression(pattern, "i")),
                    filterBuilder.Regex(x => x.FullName, new BsonRegularExpression(pattern, "i"))
                ));
            }
            filters.Add(filterBuilder.And(nameFilters));
        }

        if (!string.IsNullOrEmpty(email))
        {
            var decodedEmail = Uri.UnescapeDataString(email.Replace("+", " ")).Trim();
            filters.Add(filterBuilder.Regex(x => x.Email, new BsonRegularExpression(decodedEmail, "i")));
        }

        var filter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;

        var options = new FindOptions<User, User>
        {
            Collation = new Collation("en", strength: CollationStrength.Secondary)
        };
        var totalCount = await _users.CountDocumentsAsync(filter, new CountOptions { Collation = options.Collation });
        var users = await _users.Find(filter)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        return (users.Select(u => _mapper.Map<UserDomain>(u)), (int)totalCount);
    }
}
