using Microsoft.AspNetCore.Authorization;
using static ApiKeys.Client.Auth.ApiKeyAuthConstants;

namespace ApiKeys.Client.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyRequiredAttribute : AuthorizeAttribute
{
    public ApiKeyRequiredAttribute()
    {
        this.AuthenticationSchemes = SchemeName;
    }

    public string[]? RequiredClaims
    {
        get
        {
            if (string.IsNullOrEmpty(this.Policy))
            {
                return null;
            }

            if (!this.Policy.StartsWith(PolicyPrefix, StringComparison.Ordinal))
            {
                return null;
            }

            var claimsString = this.Policy[PolicyPrefix.Length..];
            return string.IsNullOrEmpty(claimsString)
                ? null
                : claimsString.Split('|', StringSplitOptions.RemoveEmptyEntries);
        }
        set
        {
            if (value is null || value.Length == 0)
            {
                this.Policy = null;
            }
            else
            {
                var claimsString = string.Join("|", value);
                this.Policy = PolicyPrefix + claimsString;
            }
        }
    }
}