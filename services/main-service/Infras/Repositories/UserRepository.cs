using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDomain> _users;

    public UserRepository(MongoDbService mongoDbService)
    {
        var database = mongoDbService.Database;
        _users = database.GetCollection<UserDomain>("users");
    }

    public async Task<UserDomain> CreateUserAsync(UserDomain user)
    {
        await _users.InsertOneAsync(user);
        return user;
    }
}
