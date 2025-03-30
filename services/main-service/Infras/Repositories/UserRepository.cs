using AutoMapper;
using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly IMapper _mapper;
    public UserRepository(MongoDbService mongoDbService, IMapper mapper)
    {
        var database = mongoDbService.Database;
        _users = database.GetCollection<User>("users");
        _mapper = mapper;


        // Indexing email 
        var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<User>(indexKeys, indexOptions);
        _users.Indexes.CreateOne(indexModel);
    }

    public async Task<UserDomain> CreateUserAsync(UserDomain userDomain)
    {
        // Convert Domain to Entity
        var userEntity = _mapper.Map<User>(userDomain);

        await _users.InsertOneAsync(userEntity);

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
    public async Task<UserDomain> UpdateUser(UserDomain userDomain)
    {
        var userEntity = _mapper.Map<User>(userDomain);
        var result = await _users.ReplaceOneAsync(i => i.Id == userDomain.Id, userEntity);
        if (result.MatchedCount == 0)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        return userDomain;
    }
}
