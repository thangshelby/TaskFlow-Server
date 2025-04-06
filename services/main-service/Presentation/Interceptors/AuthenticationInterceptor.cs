using Grpc.Core;
using Grpc.Core.Interceptors;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class AuthenticationInterceptor : Interceptor
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationInterceptor> _logger;

    public AuthenticationInterceptor(IConfiguration configuration, ILogger<AuthenticationInterceptor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var token = GetTokenFromMetadata(context);

        if (string.IsNullOrEmpty(token))
        {
            return await continuation(request, context);
        }
        var claimsPrincipal = DecodeJwtToken(token);

#pragma warning disable CS8601 // Possible null reference assignment.
        context.UserState["UserId"] = claimsPrincipal?.FindFirst("userId")?.Value;
#pragma warning restore CS8601 // Possible null reference assignment.

#pragma warning disable CS8601 // Possible null reference assignment.
        context.UserState["UserRole"] = claimsPrincipal?.FindFirst("role")?.Value;
#pragma warning restore CS8601 // Possible null reference assignment.


        return await continuation(request, context);
    }

    private string? GetTokenFromMetadata(ServerCallContext context)
    {
        foreach (var header in context.RequestHeaders)
        {
            if (header.Key == "cookie")
            {
                var cookies = header.Value.Split(';');
                foreach (var cookie in cookies)
                {
                    if (cookie.Trim().StartsWith("token=", StringComparison.OrdinalIgnoreCase))
                    {
                        return cookie.Substring("token=".Length).Trim();
                    }
                }
            }
        }
        return null;
    }

    private ClaimsPrincipal DecodeJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            if (tokenHandler.ReadToken(token) is not JwtSecurityToken securityToken)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token."));

            return new ClaimsPrincipal(new ClaimsIdentity(securityToken?.Claims));
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, $"Token decoding failed: {ex.Message}"));
        }
    }
}
