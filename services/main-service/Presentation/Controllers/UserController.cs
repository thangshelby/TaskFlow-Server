using FluentValidation;
using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Enums;
using MainService.Domain.UseCases;
using TaskFlow.UserService;
using BaseService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using AutoMapper;
using System.Net.Http;
using System.Text.Json;
using MainService.Domain.Interfaces;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using System.IO;
using System.Text;

public class UserController : UserService.UserServiceBase
{
    private readonly IValidator<CreateUserReq> _createUserValidator;
    private readonly IValidator<LoginUserReq> _loginUserValidator;
    private readonly IValidator<UpdateUserReq> _updateUserValidator;
    private readonly IValidator<RegisterUserReq> _registerUserValidator;
    private readonly IValidator<ChangePasswordReq> _changePasswordValidator;
    private readonly IMapper _mapper;
    private readonly UserUseCase _userUseCase;
    private readonly ILogger<UserController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAmazonKeyManagementService _kmsClient;

    public UserController(
        IValidator<CreateUserReq> createUserValidator,
        IValidator<LoginUserReq> loginUserValidator,
        IValidator<UpdateUserReq> updateUserValidator,
        IValidator<RegisterUserReq> registerUserValidator,
        IValidator<ChangePasswordReq> changePasswordValidator,
        UserUseCase userUseCase,
        ILogger<UserController> logger,
        IConfiguration configuration,
        IMapper mapper,
        IAmazonKeyManagementService kmsClient)
    {
        _createUserValidator = createUserValidator ?? throw new ArgumentNullException(nameof(createUserValidator));
        _loginUserValidator = loginUserValidator ?? throw new ArgumentNullException(nameof(loginUserValidator));
        _updateUserValidator = updateUserValidator ?? throw new ArgumentNullException(nameof(updateUserValidator));
        _registerUserValidator = registerUserValidator ?? throw new ArgumentNullException(nameof(registerUserValidator));
        _changePasswordValidator = changePasswordValidator ?? throw new ArgumentNullException(nameof(changePasswordValidator));
        _userUseCase = userUseCase ?? throw new ArgumentNullException(nameof(userUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _kmsClient = kmsClient ?? throw new ArgumentNullException(nameof(kmsClient));
    }

    public override async Task<UpdateUserRes> UpdateUser(UpdateUserReq request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;

        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Authentication is required to update a user."));   
        }

        var validationResult = await _updateUserValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var updatedUser = await _userUseCase.UpdateUserAsync(new UpdateUserParams
        {
            Id = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            Avatar = request.Avatar,
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
        var validationResult = await _createUserValidator.ValidateAsync(request);
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

    public override async Task<RegisterUserRes> Register(RegisterUserReq request, ServerCallContext context)
    {
        var validationResult = await _registerUserValidator.ValidateAsync(request);
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
            IsVerified = false,
            Avatar = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddMinutes(5), // TTL field
        });

        var userResponse = _mapper.Map<UserRes>(user);
        return new RegisterUserRes
        {
            Status = "success",
            Message = "Send mail verify successfully!"
        };
    }
    public override async Task<VerifyOTPRes> VerifyOTP(VerifyOTPReq request, ServerCallContext context)
    {
        var user = await _userUseCase.VerifyUser(request.Otp, request.Email);

        var userResponse = _mapper.Map<UserRes>(user);

        if (user.Id != null) await SetJwtToken(user, context);

        return new VerifyOTPRes
        {
            Status = "success",
            Message = "Account verified successfully!",
            Data = userResponse
        };
    }

    public override async Task<ResendOTPRes> ResendOTP(ResendOTPReq request, ServerCallContext context)
    {
        await _userUseCase.ResendOTP(request.Email);

        return new ResendOTPRes
        {
            Status = "success",
            Message = "Resend mail verify successfully!",
        };
    }

    public override async Task<LoginUserRes> Login(LoginUserReq request, ServerCallContext context)
    {
        var validationResult = await _loginUserValidator.ValidateAsync(request);
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
        await SetJwtToken(user, context);

        var userResponse = _mapper.Map<UserRes>(user);

        return new LoginUserRes
        {
            Status = "success",
            Message = "Login successfully",
            Data = userResponse
        };
    }

    public override async Task<LoginUserRes> OAuthLogin(OAuthLoginReq request, ServerCallContext context)
    {
        _logger.LogInformation("Processing OAuth login/signup. Provider: {Provider}", request.Provider);

        if (string.IsNullOrEmpty(request.Code))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Authorization code is required"));
        }

        if (string.IsNullOrEmpty(request.RedirectUri))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Redirect URI is required"));
        }

        string email = "";
        string firstName = "";
        string lastName = "";
        string avatar = "";

        try
        {
            if (request.Provider.ToLower() == "google")
            {
                var googleProfile = await ExchangeGoogleCodeAsync(request.Code, request.RedirectUri);
                email = googleProfile.Email ?? "";
                firstName = googleProfile.GivenName ?? "";
                lastName = googleProfile.FamilyName ?? "";
                avatar = googleProfile.Picture ?? "";

                if (string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(googleProfile.Name))
                {
                    var parts = googleProfile.Name.Split(' ', 2);
                    firstName = parts[0];
                    lastName = parts.Length > 1 ? parts[1] : "";
                }
            }
            else if (request.Provider.ToLower() == "github")
            {
                var githubProfile = await ExchangeGithubCodeAsync(request.Code, request.RedirectUri);
                email = githubProfile.Email ?? "";
                avatar = githubProfile.AvatarUrl ?? "";
                
                string fullName = githubProfile.Name ?? githubProfile.Login ?? "";
                var parts = fullName.Split(' ', 2);
                firstName = parts[0];
                lastName = parts.Length > 1 ? parts[1] : "";
            }
            else
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Unsupported OAuth provider: {request.Provider}"));
            }

            if (string.IsNullOrEmpty(email))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Failed to retrieve email from OAuth provider. Please make your email public on your profile or allow email access scope."));
            }

            UserDomain? user = await _userUseCase.FindUserAsync(new UserQueryParams { Email = email });
            bool isNewUser = false;

            if (user == null)
            {
                _logger.LogInformation("OAuth user {Email} not found. Auto-registering new account.", email);
                isNewUser = true;

                user = new UserDomain
                {
                    FirstName = !string.IsNullOrEmpty(firstName) ? firstName : "User",
                    LastName = lastName,
                    Email = email,
                    Password = Guid.NewGuid().ToString("N"), // Safe random placeholder password
                    Role = UserRole.User,
                    IsVerified = true, // OAuth emails are verified by Google/GitHub
                    Avatar = avatar,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.MaxValue
                };

                user = await _userUseCase.CreateOAuthUserAsync(user);
            }
            else
            {
                _logger.LogInformation("OAuth user {Email} found. Initiating login session.", email);
                
                // Verify account if it was in registered state
                if (!user.IsVerified)
                {
                    user.IsVerified = true;
                    user.ExpiredAt = DateTime.MaxValue;
                    await _userUseCase.UpdateUserAsync(new UpdateUserParams
                    {
                        Id = user.Id,
                        IsVerified = true,
                        ExpiredAt = DateTime.MaxValue
                    });
                }
                
                // Update empty avatar with the one from OAuth provider
                if (string.IsNullOrEmpty(user.Avatar) && !string.IsNullOrEmpty(avatar))
                {
                    user.Avatar = avatar;
                    await _userUseCase.UpdateUserAsync(new UpdateUserParams
                    {
                        Id = user.Id,
                        Avatar = avatar
                    });
                }
            }

            await SetJwtToken(user, context);
            var userResponse = _mapper.Map<UserRes>(user);

            return new LoginUserRes
            {
                Status = "success",
                Message = isNewUser ? "Account created and logged in via OAuth" : "Logged in successfully via OAuth",
                Data = userResponse
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth login process for {Provider}", request.Provider);
            throw new RpcException(new Status(StatusCode.Internal, $"OAuth authentication failed: {ex.Message}"));
        }
    }

    private async Task<GoogleUserProfile> ExchangeGoogleCodeAsync(string code, string redirectUri)
    {
        string clientId = _configuration["GOOGLE_CLIENT_ID"] ?? "";
        string clientSecret = _configuration["GOOGLE_CLIENT_SECRET"] ?? "";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new Exception("Google Client ID or Client Secret is not configured in the backend environment variables.");
        }

        using var client = new HttpClient();
        var tokenRequestParams = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        };

        var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequestParams));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Google token exchange failed. Status: {Status}, Body: {Content}", response.StatusCode, content);
            throw new Exception("Failed to exchange Authorization Code with Google.");
        }

        var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(content);
        if (tokenData == null || string.IsNullOrEmpty(tokenData.access_token))
        {
            throw new Exception("Google token response did not contain a valid access_token.");
        }

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.access_token);
        var profileResponse = await client.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        var profileContent = await profileResponse.Content.ReadAsStringAsync();

        if (!profileResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Google userinfo request failed. Status: {Status}, Body: {Content}", profileResponse.StatusCode, profileContent);
            throw new Exception("Failed to retrieve user profile info from Google.");
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var profile = JsonSerializer.Deserialize<GoogleUserProfile>(profileContent, options);
        if (profile == null)
        {
            throw new Exception("Deserialization of Google user profile failed.");
        }

        return profile;
    }

    private async Task<GithubUserProfile> ExchangeGithubCodeAsync(string code, string redirectUri)
    {
        string clientId = _configuration["GITHUB_CLIENT_ID"] ?? "";
        string clientSecret = _configuration["GITHUB_CLIENT_SECRET"] ?? "";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new Exception("GitHub Client ID or Client Secret is not configured in the backend environment variables.");
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "TaskFlow-App");

        var tokenRequestParams = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "code", code },
            { "redirect_uri", redirectUri }
        };

        var response = await client.PostAsync("https://github.com/login/oauth/access_token", new FormUrlEncodedContent(tokenRequestParams));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("GitHub token exchange failed. Status: {Status}, Body: {Content}", response.StatusCode, content);
            throw new Exception("Failed to exchange Authorization Code with GitHub.");
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var tokenData = JsonSerializer.Deserialize<GithubTokenResponse>(content, options);
        if (tokenData == null || string.IsNullOrEmpty(tokenData.access_token))
        {
            throw new Exception("GitHub token response did not contain a valid access_token.");
        }

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.access_token);
        var profileResponse = await client.GetAsync("https://api.github.com/user");
        var profileContent = await profileResponse.Content.ReadAsStringAsync();

        if (!profileResponse.IsSuccessStatusCode)
        {
            _logger.LogError("GitHub user profile request failed. Status: {Status}, Body: {Content}", profileResponse.StatusCode, profileContent);
            throw new Exception("Failed to retrieve user profile info from GitHub.");
        }

        var profile = JsonSerializer.Deserialize<GithubUserProfile>(profileContent, options);
        if (profile == null)
        {
            throw new Exception("Deserialization of GitHub user profile failed.");
        }

        // If email is private, fetch it from /user/emails
        if (string.IsNullOrEmpty(profile.Email))
        {
            _logger.LogInformation("GitHub profile email is private. Fetching from /user/emails endpoint.");
            var emailsResponse = await client.GetAsync("https://api.github.com/user/emails");
            var emailsContent = await emailsResponse.Content.ReadAsStringAsync();

            if (emailsResponse.IsSuccessStatusCode)
            {
                var emails = JsonSerializer.Deserialize<List<GithubEmail>>(emailsContent, options);
                var primaryEmail = emails?.FirstOrDefault(e => e.Primary && e.Verified)?.Email 
                                   ?? emails?.FirstOrDefault(e => e.Verified)?.Email 
                                   ?? emails?.FirstOrDefault()?.Email;
                
                if (!string.IsNullOrEmpty(primaryEmail))
                {
                    profile.Email = primaryEmail;
                }
            }
        }

        return profile;
    }

    private class GoogleTokenResponse
    {
        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public int expires_in { get; set; }
        public string? id_token { get; set; }
        public string? scope { get; set; }
    }

    private class GoogleUserProfile
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Picture { get; set; }
    }

    private class GithubTokenResponse
    {
        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public string? scope { get; set; }
    }

    private class GithubUserProfile
    {
        public string? Login { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
    }

    private class GithubEmail
    {
        public string? Email { get; set; }
        public bool Primary { get; set; }
        public bool Verified { get; set; }
    }

    public override async Task<ListUsersRes> ListUsers(ListUsersReq request, ServerCallContext context)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var limit = request.Limit <= 0 ? 10 : request.Limit;

        var result = await _userUseCase.SearchUsersAsync(new SearchUserQueryParams
        {
            Name = request.Name,
            Email = request.Email,
            UserIds = request.UserIds.ToList(),
            ProjectId = request.ProjectId,
            Page = (int)page,
            Limit = (int)limit,
        }
        );
        var users = result.Users;
        var totalCount = result.TotalCount;

        var response = new ListUsersRes
        {
            Status = "success",
            Pagination = new PaginationRes
            {
                TotalItems = totalCount,
                CurrentPage = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
            }
        };

        response.Data.AddRange(users.Select(u => _mapper.Map<UserRes>(u)));
        return response;
    }

    public override async Task<GetUserRes> GetByEmail(GetUserByEmailReq request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Email is required"));
            }

            _logger.LogInformation("Getting user by email: {Email}", request.Email);
            var user = await _userUseCase.FindUserAsync(new UserQueryParams { Email = request.Email });

            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"User with email {request.Email} not found"));
            }

            return new GetUserRes
            {
                Status = "success",
                Data = _mapper.Map<UserRes>(user)
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", request.Email);
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving user"));
        }
    }
    public override async Task<LogoutUserRes> Logout(LogoutUserReq request, ServerCallContext context)
    {
        var metadata = new Metadata
        {
            { "Set-Cookie", "token=; Path=/; HttpOnly; Secure; SameSite=None; Max-Age=0" }
        };
        await context.WriteResponseHeadersAsync(metadata);

        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("User {UserId} logged out successfully", userId);
        }

        return new LogoutUserRes
        {
            Status = "success",
            Message = "Logged out successfully"
        };
    }

    // TODO: Move to stat service
    public override async Task<GetStatsRes> GetStats(GetStatsReq request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;

        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User is not authenticated."));
        }
        var result = await _userUseCase.GetStats(request.Id, request.IsSprintId);
        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve stats."));
        }

        return new GetStatsRes
        {
            Status = "success",
            Message = "Get stats successfully",
            Data = result
        };
    }

    public override async Task<ChangePasswordRes> ChangePassword(ChangePasswordReq request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User is not authenticated"));
        }

        // Ensure user can only change their own password
        if (userId != request.UserId)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Cannot change another user's password"));
        }

        var validationResult = await _changePasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        try
        {
            await _userUseCase.ChangePassword(request.UserId, request.OldPassword, request.NewPassword);

            return new ChangePasswordRes
            {
                Status = "success",
                Message = "Password changed successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, "Error changing password"));
        }
    }

    private async Task SetJwtToken(UserDomain user, ServerCallContext context)
    {
        string jwtToken = await CreateJwtTokenWithKmsAsync(user);

        var metadata = new Metadata
        {
            { "Set-Cookie", $"token={jwtToken}; Path=/; HttpOnly; Secure; SameSite=None" }
        };
        await context.WriteResponseHeadersAsync(metadata);
    }

    private async Task<string> CreateJwtTokenWithKmsAsync(UserDomain user)
    {
        string? kmsKeyId = _configuration["KMS_KEY_ID"];
        if (string.IsNullOrEmpty(kmsKeyId))
        {
            throw new Exception("KMS Key ID not found in configuration.");
        }

        // 1. Create Header
        var header = new { alg = "RS256", typ = "JWT" };
        string encodedHeader = Base64UrlEncoder.Encode(JsonSerializer.Serialize(header));

        // 2. Create Payload
        var payload = new
        {
            sub = user.Id,
            name = user.Email,
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
            role = user.Role.ToString(),
            userId = user.Id,
            isVerified = user.IsVerified.ToString().ToLower()
        };
        string encodedPayload = Base64UrlEncoder.Encode(JsonSerializer.Serialize(payload));

        // 3. Prepare message to sign
        string messageToSign = $"{encodedHeader}.{encodedPayload}";
        byte[] messageBytes = Encoding.UTF8.GetBytes(messageToSign);

        // 4. Hash message with SHA256
        byte[] digest;
        using (var sha256 = SHA256.Create())
        {
            digest = sha256.ComputeHash(messageBytes);
        }

        // 5. Call AWS KMS to sign
        var signRequest = new SignRequest
        {
            KeyId = kmsKeyId,
            Message = new MemoryStream(digest),
            MessageType = MessageType.DIGEST,
            SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PKCS1_V1_5_SHA_256
        };

        var signResponse = await _kmsClient.SignAsync(signRequest);

        // 6. Encode signature and form complete JWT
        string signature = Base64UrlEncoder.Encode(signResponse.Signature.ToArray());
        return $"{messageToSign}.{signature}";
    }
}