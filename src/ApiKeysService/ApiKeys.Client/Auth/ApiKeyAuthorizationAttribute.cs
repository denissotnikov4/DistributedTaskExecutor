using Microsoft.AspNetCore.Authorization;

namespace ApiKeys.Client.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyRequiredAttribute : AuthorizeAttribute
{
    public ApiKeyRequiredAttribute()
    {
        AuthenticationSchemes = ApiKeyAuthConstants.SchemeName;
    }

    public string[]? RequiredClaims
    {
        get
        {
            if (string.IsNullOrEmpty(Policy))
            {
                return null;
            }

            if (!Policy.StartsWith(ApiKeyAuthConstants.PolicyPrefix, StringComparison.Ordinal))
            {
                return null;
            }

            var claimsString = Policy[ApiKeyAuthConstants.PolicyPrefix.Length..];
            return string.IsNullOrEmpty(claimsString)
                ? null
                : claimsString.Split('|', StringSplitOptions.RemoveEmptyEntries);
        }
        set
        {
            if (value is null || value.Length == 0)
            {
                Policy = null;
            }
            else
            {
                var claimsString = string.Join("|", value);
                Policy = ApiKeyAuthConstants.PolicyPrefix + claimsString;
            }
        }
    }
}