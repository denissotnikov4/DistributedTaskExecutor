using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApiKeys.Logic.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly string key;
    private readonly string issuer;
    private readonly string audience;
    private readonly int expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
        issuer = configuration["Jwt:Issuer"] ?? "ApiKeysService";
        audience = configuration["Jwt:Audience"] ?? "ApiKeysService";
        expirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var exp)
            ? exp
            : 60;
    }

    public string GenerateToken(IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var signingKey = new SymmetricSecurityKey(keyBytes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateToken(string username, IEnumerable<string> claims)
    {
        var claimList = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, username),
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var claim in claims)
        {
            claimList.Add(new Claim("claim", claim));
        }

        return GenerateToken(claimList);
    }
}