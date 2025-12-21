using System.Security.Claims;

namespace ApiKeys.Logic.Services.Auth;

public interface IJwtTokenService
{
    string GenerateToken(IEnumerable<Claim> claims);
    string GenerateToken(string username, IEnumerable<string> claims);
}