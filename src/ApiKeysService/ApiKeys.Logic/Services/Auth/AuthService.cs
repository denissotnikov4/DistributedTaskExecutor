using ApiKeys.Logic.Configuration;
using ApiKeys.Logic.Models;
using DistributedTaskExecutor.Core.Results;
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
    public Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var adminUsername = this.adminUserOptions.Username;
        var adminPassword = this.adminUserOptions.Password;
        
        if (!adminUsername.Equals(request.Username, StringComparison.OrdinalIgnoreCase)
            || adminPassword != request.Password)
        {
            logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            return Task.FromResult<Result<LoginResponse>>(ServiceError.Unauthorized("Invalid username or password"));
        }

        var token = tokenProvider.GenerateToken(request.Username, this.adminUserOptions.Claims);

        logger.LogInformation("User {Username} logged in successfully", request.Username);

        return Task.FromResult<Result<LoginResponse>>(new LoginResponse(token));
    }
}