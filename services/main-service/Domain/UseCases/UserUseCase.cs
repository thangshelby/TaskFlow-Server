using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Packages;

namespace MainService.Domain.UseCases;

public class UserUseCase
{
    private readonly IUserRepository _userRepository;

    public UserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDomain> CreateUser(UserDomain userBody)
    {
        var user = await _userRepository.FindUserAsync(new UserQueryParams
        {
            Email = userBody.Email
        });

        if (user != null)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Email has been used!!"));
        }

        userBody.Password = PasswordHasher.HashPassword(userBody.Password);
        return await _userRepository.CreateUserAsync(userBody);
    }

    public async Task<UserDomain> LoginUser(LoginReqParams param)
    {
        var user = await _userRepository.FindUserAsync(new UserQueryParams
        {
            Email = param.Email
        });

        if (user == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found!"));


        if (!PasswordHasher.ValidatePassword(param.Password, user.Password))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid password!"));


        return user;
    }
    public async Task<UserDomain> GetById(string userId)
    {
        var user = await _userRepository.FindUserAsync(new UserQueryParams
        {
            UserId = userId
        });

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found!"));
        }

        return user;
    }
    public async Task<UserDomain> UpdateUser(UserDomain user)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "userId not found!!"));
        }

        var existingUser = await _userRepository.FindUserAsync(new UserQueryParams
        {
            UserId = user.Id
        });

        if (existingUser == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {user.Id} not found"));


        if (!string.IsNullOrEmpty(user.FirstName))
            existingUser.FirstName = user.FirstName;

        if (!string.IsNullOrEmpty(user.LastName))
            existingUser.LastName = user.LastName;

        if (!string.IsNullOrEmpty(user.Email))
            existingUser.Email = user.Email;

        if (!string.IsNullOrEmpty(user.Password))
            existingUser.Password = PasswordHasher.HashPassword(user.Password);

        existingUser.UpdatedAt = DateTime.UtcNow;

        return await _userRepository.UpdateUser(existingUser);
    }

}
