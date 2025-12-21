using Microsoft.AspNetCore.Authorization;

namespace ApiKeys.Client.Auth;

public class ApiKeyClaimsAuthorizationHandler : AuthorizationHandler<ApiKeyClaimsRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApiKeyClaimsRequirement requirement)
    {
        if (requirement.RequiredClaims is null || requirement.RequiredClaims.Length == 0)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var apiKeyClaims = context.User
            .FindAll(requirement.ClaimType)
            .Select(c => c.Value)
            .ToHashSet();

        var hasRequiredClaim = requirement.RequiredClaims.Any(claim => apiKeyClaims.Contains(claim));

        if (hasRequiredClaim)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

public class ApiKeyClaimsRequirement(
    string[]? requiredClaims,
    string claimType = ApiKeyAuthConstants.ApiKeyClaimType) : IAuthorizationRequirement
{
    public string[]? RequiredClaims { get; } = requiredClaims;
    public string ClaimType { get; } = claimType;
}