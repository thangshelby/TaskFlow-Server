using MainService.Domain.Entities;
using MainService.Domain.Interfaces;

namespace MainService.Domain.UseCases;

public class UserUseCase
{
    private readonly IUserRepository _userRepository;

    public UserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDomain> CreateUser(UserDomain user)
    {
        // logic business

        // Save the user
        return await _userRepository.CreateUserAsync(user);
    }
}
