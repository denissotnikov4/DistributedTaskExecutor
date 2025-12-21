using Microsoft.AspNetCore.Authentication;

namespace ApiKeys.Client.Auth;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string SchemeName { get; set; } = ApiKeyAuthConstants.SchemeName;

    public string HeaderName { get; set; } = ApiKeyAuthConstants.HeaderName;

    public string? QueryParameterName { get; set; } = ApiKeyAuthConstants.QueryParameterName;

    public string ApiKeyIdClaimType { get; set; } = ApiKeyAuthConstants.ApiKeyIdClaimType;

    public string ApiKeyClaimType { get; set; } = ApiKeyAuthConstants.ApiKeyClaimType;
}