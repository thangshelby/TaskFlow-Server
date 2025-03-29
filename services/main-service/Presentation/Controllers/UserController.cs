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

public class UserController : UserService.UserServiceBase
{
    private readonly IValidator<CreateUserReq> _createUserValidator;
    private readonly IValidator<LoginUserReq> _loginUserValidator;
    private readonly UserUseCase _userUseCase;
    private readonly ILogger<UserController> _logger;
    private readonly IConfiguration _configuration;
    public UserController(
        IValidator<CreateUserReq> createUserValidator,
        IValidator<LoginUserReq> loginUserValidator,
        UserUseCase userUseCase,
        ILogger<UserController> logger, IConfiguration configuration)
        => (_createUserValidator, _loginUserValidator, _userUseCase, _logger, _configuration)
        = (createUserValidator, loginUserValidator, userUseCase, logger, configuration);


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
        string privateKeyPem = _configuration["JWT_SECRET"];
        _logger.LogInformation("Private Key: {privateKeyPem}.");

        if (string.IsNullOrEmpty(privateKeyPem))
        {
            throw new Exception("Private key not found in configuration.");
        }

        _logger.LogInformation("Private Key Loaded Successfully.");

        // Convert PEM string to RSA key
        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem.ToCharArray());

        // Create security key with RSA
        var securityKey = new RsaSecurityKey(rsa);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        // JWT Payload
        var claims = new[]{
        new Claim(JwtRegisteredClaimNames.Sub, "1234567890"),
        new Claim(JwtRegisteredClaimNames.Name, request.Email),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new Claim(JwtRegisteredClaimNames.Iss, "127.0.0.1"),
        new Claim("role", "admin_role")
    };

        // Create JWT token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "127.0.0.1",
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        // Set Token in gRPC Headers
        Metadata metadata = new Metadata
    {
        { "Set-Cookie", $"token={jwtToken}; Path=/; HttpOnly; Secure; SameSite=None" }
    };
        await context.WriteResponseHeadersAsync(metadata);

        return new LoginUserRes
        {
            Status = "Login successfully",
        };
    }
}
