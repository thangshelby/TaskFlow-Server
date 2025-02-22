using MainService.Domain.Entities;
namespace MainService.Domain.Interfaces;
public interface IUserRepository
{
    Task<UserDomain> CreateUserAsync(UserDomain user);
    Task<UserDomain?> FindUserAsync(string userId);
}
