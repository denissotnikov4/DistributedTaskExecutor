using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiKeys.Logic.Services.Auth;

public interface ITokenProvider
{
    string GenerateToken(string username, IEnumerable<string> additionalClaims);
}

public class JwtTokenProvider(IOptions<JwtOptions> options) : ITokenProvider
{
    private readonly JwtOptions options = options.Value;

    public string GenerateToken(string username, IEnumerable<string> additionalClaims)
    {
        var claims = CreateClaims(username, additionalClaims);
        var signingCredentials = CreateSigningCredentials();
        var token = CreateJwtToken(claims, signingCredentials);

        var jwtTokenHandler = new JwtSecurityTokenHandler();
        return jwtTokenHandler.WriteToken(token);
    }

    private JwtSecurityToken CreateJwtToken(
        List<Claim> claims,
        SigningCredentials signingCredentials)
    {
        return new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(options.ExpiryMinutes),
            signingCredentials: signingCredentials);
    }

    private SigningCredentials CreateSigningCredentials()
    {
        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(options.Secret)),
            SecurityAlgorithms.HmacSha256);
    }

    private static List<Claim> CreateClaims(string username, IEnumerable<string> additionalClaims)
    {
        var claimList = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, username),
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var claim in additionalClaims)
        {
            claimList.Add(new Claim("claim", claim));
        }

        return claimList;
    }
}