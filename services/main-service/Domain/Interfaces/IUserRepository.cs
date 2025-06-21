using MainService.Domain.Entities;
namespace MainService.Domain.Interfaces;
public interface IUserRepository
{
    Task<UserDomain> CreateUserAsync(UserDomain user);
    Task<UserDomain?> FindUserAsync(UserQueryParams query);

    Task<UserDomain> UpdateUser(UserDomain user);
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
    public List<string>? UserIds { get; set; }
    public int? Page { get; set; }
    public int? Limit { get; set; }
}