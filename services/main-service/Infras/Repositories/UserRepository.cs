using AutoMapper;
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

    public async Task<UserDomain?> FindUserAsync(string userId)
    {
        var userEntity = await _users
            .Find(x => x.Id == userId)
            .FirstOrDefaultAsync();

        if (userEntity == null)
            return null;

        return _mapper.Map<UserDomain>(userEntity);
    }


}
