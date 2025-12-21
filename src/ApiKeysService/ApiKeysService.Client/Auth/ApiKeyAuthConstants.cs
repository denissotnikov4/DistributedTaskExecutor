namespace ApiKeysService.Client.Auth;

public static class ApiKeyAuthConstants
{
    internal const string PolicyPrefix = "ApiKeyClaims_";

    public const string SchemeName = "ApiKey";
    
    public const string HeaderName = "X-Api-Key";
    
    public const string QueryParameterName = "apiKey";

    public const string ApiKeyIdClaimType = "ApiKeyId";

    public const string ApiKeyClaimType = "ApiKeyClaim";
}