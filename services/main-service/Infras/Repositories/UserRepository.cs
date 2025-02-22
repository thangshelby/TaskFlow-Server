using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbService mongoDbService)
    {
        var database = mongoDbService.Database;
        _users = database.GetCollection<User>("users");
    }

    public async Task<UserDomain> CreateUserAsync(UserDomain userDomain)
    {
        // Convert Domain to Entity
        var userEntity = MapToEntity(userDomain);

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

        return MapToDomain(userEntity);
    }

    // Helper Method: Map Domain to Entity
    private static User MapToEntity(UserDomain domain)
    {
        return new User
        {
            Id = domain.Id,
            Username = domain.Username,
            Email = domain.Email,
            Password = domain.Password,
            CreatedAt = domain.CreatedAt
        };
    }

    // Helper Method: Map Entity to Domain
    private static UserDomain MapToDomain(User entity)
    {
        return new UserDomain
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            Password = entity.Password,
            CreatedAt = entity.CreatedAt
        };
    }
}
