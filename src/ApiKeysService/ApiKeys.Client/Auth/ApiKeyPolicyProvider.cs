using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ApiKeys.Client.Auth;

public class ApiKeyPolicyProvider(
    IOptions<AuthorizationOptions> authorizationOptions,
    IOptionsMonitor<ApiKeyAuthenticationOptions> apiKeyOptions
)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider fallbackPolicyProvider = new(authorizationOptions);
    private readonly ApiKeyAuthenticationOptions options = apiKeyOptions.CurrentValue;

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(ApiKeyAuthConstants.PolicyPrefix, StringComparison.Ordinal))
        {
            var claimsString = policyName[ApiKeyAuthConstants.PolicyPrefix.Length..];
            if (!string.IsNullOrEmpty(claimsString))
            {
                var requiredClaims = claimsString.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (requiredClaims.Length > 0)
                {
                    var claimType = options.ApiKeyClaimType;

                    var policy = new AuthorizationPolicyBuilder(options.SchemeName)
                        .AddRequirements(new ApiKeyClaimsRequirement(requiredClaims, claimType))
                        .Build();

                    return Task.FromResult<AuthorizationPolicy?>(policy);
                }
            }
        }

        return fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}