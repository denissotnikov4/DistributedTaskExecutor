using ApiKeys.Logic.Configuration;
using ApiKeys.Logic.Models;
using Core.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiKeys.Logic.Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
}

public class AuthService(
    ITokenProvider tokenProvider,
    IOptions<AdminUserOption> adminUserOptions,
    ILogger<AuthService> logger
)
    : IAuthService
{
    private readonly AdminUserOption adminUserOptions = adminUserOptions.Value;

    // note: пока так, для демонстрации пойдет
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var adminUsername = adminUserOptions.Username;
        var adminPassword = adminUserOptions.Password;
        
        if (!adminUsername.Equals(request.Username, StringComparison.OrdinalIgnoreCase)
            || adminPassword != request.Password)
        {
            logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            return ServiceError.Unauthorized("Invalid username or password");
        }

        var token = tokenProvider.GenerateToken(request.Username, adminUserOptions.Claims);

        logger.LogInformation("User {Username} logged in successfully", request.Username);

        return new LoginResponse(token);
    }
}