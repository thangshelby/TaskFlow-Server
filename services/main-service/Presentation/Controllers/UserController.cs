using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Enums;
using MainService.Domain.UseCases;
using TaskFlow.UserService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using AutoMapper;
using MainService.Domain.Interfaces;
using Google.Protobuf.WellKnownTypes;

public class UserController : UserService.UserServiceBase
{
    private readonly IValidator<CreateUserReq> _createUserValidator;
    private readonly IValidator<LoginUserReq> _loginUserValidator;
    private readonly IValidator<UpdateUserReq> _updateUserValidator;
    private readonly IValidator<RegisterUserReq> _registerUserValidator;
    private readonly IMapper _mapper;

    private readonly UserUseCase _userUseCase;
    private readonly ILogger<UserController> _logger;
    private readonly IConfiguration _configuration;
    public UserController(
        IValidator<CreateUserReq> createUserValidator,
        IValidator<LoginUserReq> loginUserValidator,
        IValidator<UpdateUserReq> updateUserValidator,
        IValidator<RegisterUserReq> registerUserValidator,
        UserUseCase userUseCase,
        ILogger<UserController> logger, IConfiguration configuration,
        IMapper mapper
        )
        => (_createUserValidator, _loginUserValidator, _updateUserValidator, _registerUserValidator, _userUseCase, _logger, _configuration, _mapper)
        = (createUserValidator, loginUserValidator, updateUserValidator, registerUserValidator, userUseCase, logger, configuration, mapper);

    public override async Task<UpdateUserRes> UpdateUser(UpdateUserReq request, ServerCallContext context)
    {
        ValidationResult validationResult = await _updateUserValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }
        var updatedUser = await _userUseCase.UpdateUser(new UserDomain
        {
            Id = request.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
        });

        var userResponse = _mapper.Map<UserRes>(updatedUser);


        return new UpdateUserRes
        {
            Status = "Updated successfully",
            Data = userResponse
        };
    }
    public override async Task<CreateUserRes> CreateUser(CreateUserReq request, ServerCallContext context)
    {
        ValidationResult validationResult = await _createUserValidator.ValidateAsync(request);
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
            Role = System.Enum.TryParse(request.Role, out UserRole parsedRole) ? parsedRole : UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var userResponse = _mapper.Map<UserRes>(user);
        return new CreateUserRes
        {
            Status = "User created successfully",
            Data = userResponse
        };
    }
    public override async Task<GetUserRes> GetMe(GetUserReq request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        // var userRole = context.UserState.ContainsKey("UserRole") ? context.UserState["UserRole"] as string : null;

        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User is not authenticated."));
        }
        var user = await _userUseCase.GetById(userId);


        var userResponse = _mapper.Map<UserRes>(user);

        return new GetUserRes
        {
            Status = "User fetched successfully",
            Data = userResponse
        };
    }

    public override async Task<GetUserRes> GetById(GetUserReq request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "User ID is required."));
        }

        var user = await _userUseCase.GetById(request.UserId);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
        }

        var userResponse = _mapper.Map<UserRes>(user);

        return new GetUserRes
        {
            Status = "User fetched successfully",
            Data = userResponse
        };
    }

    public override async Task<CreateUserRes> Register(RegisterUserReq request, ServerCallContext context)
    {
        ValidationResult validationResult = await _registerUserValidator.ValidateAsync(request);
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
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        if (user.Id == null) throw new Exception("Can't happen");
        await SetJwtToken(user.Id, user.Role, request.Email, context);

        var userResponse = _mapper.Map<UserRes>(user);
        return new CreateUserRes
        {
            Status = "User created successfully",
            Data = userResponse
        };
    }
    public override async Task<LoginUserRes> Login(LoginUserReq request, ServerCallContext context)
    {
        ValidationResult validationResult = await _loginUserValidator.ValidateAsync(request);
        if (!validationResult.IsValid)  
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var user = await _userUseCase.LoginUser(new LoginReqParams
        {
            Email = request.Email,
            Password = request.Password
        });

        if (user.Id == null) throw new Exception("Can't happen");
        await SetJwtToken(user.Id, user.Role, request.Email, context);

        var userResponse = _mapper.Map<UserRes>(user);

        return new LoginUserRes
        {
            Status = "success",
            Message = "Login successfully",
            Data = userResponse
        };
    }

    public override async Task<ListUsersRes> ListUsers(ListUsersReq request, ServerCallContext context)
    {
        _logger.LogInformation("Listing users with keyword: {Keyword}, page: {Page}, limit: {Limit}", request.Keyword, request.Page, request.Limit);
        try
        {
            if (request.Page <= 0) request.Page = 1;
            if (request.Limit <= 0) request.Limit = 10;

            var result = await _userUseCase.SearchUsersAsync(request.Keyword, (int)request.Page, (int)request.Limit);
            var users = result.Users;
            var totalCount = result.TotalCount;

            var response = new ListUsersRes
            {
                Status = "success",
                Pagination = new PaginationRes
                {
                    TotalItems = totalCount,
                    CurrentPage = request.Page,
                    Limit = request.Limit,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit)
                }
            };

            response.Data.AddRange(users.Select(u => _mapper.Map<UserRes>(u)));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with keyword {Keyword}", request.Keyword);
            throw new RpcException(new Status(StatusCode.Internal, "Error searching users"));
        }
    }

    private async Task SetJwtToken(string userId, UserRole role, string email, ServerCallContext context)
    {
        string? privateKeyPem = _configuration["JWT_SECRET"];
        if (string.IsNullOrEmpty(privateKeyPem))
        {
            throw new Exception("Private key not found in configuration.");
        }

        string jwtToken = CreateJwtToken(userId, role, email, privateKeyPem);

        Metadata metadata = new Metadata
        {
            { "Set-Cookie", $"token={jwtToken}; Path=/; HttpOnly; Secure; SameSite=None" }
        };
        await context.WriteResponseHeadersAsync(metadata);
    }
    private static string CreateJwtToken(string userId, UserRole role, string email, string privateKeyPem)
    {


        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem.ToCharArray());

        var securityKey = new RsaSecurityKey(rsa);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, "1234567890"),
        new Claim(JwtRegisteredClaimNames.Name, email),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new Claim(JwtRegisteredClaimNames.Iss, "127.0.0.1"),
        new Claim("role", "admin_role"),
        new Claim("userId", userId)
    };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = "127.0.0.1",
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
