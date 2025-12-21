using System.Security.Claims;

namespace ApiKeysService.Client.Auth;

public static class ApiKeyClaimsExtensions
{
    public static Guid? GetApiKeyId(
        this ClaimsPrincipal principal,
        string claimType = ApiKeyAuthConstants.ApiKeyIdClaimType)
    {
        var claim = principal.FindFirst(claimType);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    public static IEnumerable<string> GetApiKeyClaims(
        this ClaimsPrincipal principal,
        string claimType = ApiKeyAuthConstants.ApiKeyClaimType)
    {
        return principal.FindAll(claimType).Select(c => c.Value);
    }

    public static bool HasApiKeyClaim(
        this ClaimsPrincipal principal,
        string claim,
        string claimType = ApiKeyAuthConstants.ApiKeyClaimType)
    {
        return principal.HasClaim(claimType, claim);
    }

    public static bool HasAnyApiKeyClaim(
        this ClaimsPrincipal principal,
        IEnumerable<string> claims,
        string claimType = ApiKeyAuthConstants.ApiKeyClaimType)
    {
        var apiKeyClaims = principal.GetApiKeyClaims(claimType).ToHashSet();
        return claims.Any(claim => apiKeyClaims.Contains(claim));
    }
}