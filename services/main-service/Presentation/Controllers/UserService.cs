using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Enums;
using MainService.Domain.UseCases;
using TaskFlow.UserService;

public class UserController : UserService.UserServiceBase
{
    private readonly IValidator<CreateUserReq> _createUserValidator;
    private readonly IValidator<LoginUserReq> _loginUserValidator;
    private readonly UserUseCase _userUseCase;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IValidator<CreateUserReq> createUserValidator,
        IValidator<LoginUserReq> loginUserValidator,
        UserUseCase userUseCase,
        ILogger<UserController> logger)
        => (_createUserValidator, _loginUserValidator, _userUseCase, _logger)
        = (createUserValidator, loginUserValidator, userUseCase, logger);


    public override async Task<CreateUserRes> CreateUser(CreateUserReq request, ServerCallContext context)
    {
        ValidationResult validationResult = await _createUserValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = await _userUseCase.CreateUser(new UserDomain
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = hashedPassword,
            Role = Enum.TryParse(request.Role, out UserRole parsedRole) ? parsedRole : UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return new CreateUserRes
        {
            Status = "User created successfully",
            Data = new UserRes
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt.ToString("o"),
                UpdatedAt = user.UpdatedAt.ToString("o"),
            }
        };
    }
    public override async Task<LoginUserRes> Login(LoginUserReq request, ServerCallContext context)
    {
        ValidationResult validationResult = await _loginUserValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {

            throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        return new LoginUserRes
        {
            Status = "Login successfully",
        };
    }
}
