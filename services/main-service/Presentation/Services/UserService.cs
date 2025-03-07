using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Enums;
using MainService.Domain.UseCases;
using MainService.Infras.Entities;
using TaskFlow.UserService;

public class UserServiceImpl : UserService.UserServiceBase
{

    private readonly IValidator<CreateUserReq> _validator;
    private readonly UserUseCase _userUseCase;
    private readonly ILogger<UserServiceImpl> _logger;
    public UserServiceImpl(IValidator<CreateUserReq> validator, UserUseCase userUseCase, ILogger<UserServiceImpl> logger)
    {
        _validator = validator;
        _userUseCase = userUseCase;
        _logger = logger;
    }

    public override async Task<CreateUserRes> CreateUser(CreateUserReq request, ServerCallContext context)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }
        var user = await _userUseCase.CreateUser(new UserDomain
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
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
            }
        };
    }

}
