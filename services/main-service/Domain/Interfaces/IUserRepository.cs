using MainService.Domain.Entities;
using MainService.Domain.Enums;
namespace MainService.Domain.Interfaces;

public interface IUserRepository
{
    Task<UserDomain> CreateUserAsync(UserDomain user);
    Task<UserDomain?> FindUserAsync(UserQueryParams query);

    Task<UserDomain> UpdateUserAsync(UpdateUserParams param);
    Task<(IEnumerable<UserDomain> Users, int TotalCount)> SearchUsersAsync(SearchUserQueryParams param);
}

public class LoginReqParams
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserQueryParams
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
}

public class SearchUserQueryParams
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ProjectId { get; set; }
    public List<string>? UserIds { get; set; }
    public int? Page { get; set; }
    public int? Limit { get; set; }
}

public class UpdateUserParams
{
    public required string Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Avatar { get; set; }
    public bool? IsVerified { get; set; }
    public UserRole? Role { get; set; }
}