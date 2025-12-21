using ApiKeys.Logic.Models;
using Core.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiKeys.Logic.Services.Auth;

public class AuthService(
    IJwtTokenService jwtTokenService,
    IConfiguration configuration,
    ILogger<AuthService> logger
)
    : IAuthService
{
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var users = configuration.GetSection("Auth:Users").Get<List<UserConfig>>()
                    ?? new List<UserConfig>();

        var user = users.FirstOrDefault(u =>
            u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == request.Password);

        if (user == null)
        {
            logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            return ServiceError.Unauthorized("Invalid username or password");
        }

        var claims = new HashSet<string> { "ManageApiKey" };
        if (user.Claims != null)
        {
            foreach (var claim in user.Claims)
            {
                claims.Add(claim);
            }
        }

        var token = jwtTokenService.GenerateToken(request.Username, claims);

        logger.LogInformation("User {Username} logged in successfully", request.Username);

        return new LoginResponse(token);
    }

    private class UserConfig
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<string>? Claims { get; set; }
    }
}