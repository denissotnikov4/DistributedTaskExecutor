using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeysService.Client.Auth;

public static class ApiKeyAuthExtensions
{
    public static AuthenticationBuilder AddApiKeyAuthentication(
        this IServiceCollection services,
        Action<ApiKeyAuthenticationOptions>? configureOptions = null)
    {
        services.Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IAuthorizationHandler, ApiKeyClaimsAuthorizationHandler>();

        services.AddSingleton<IAuthorizationPolicyProvider, ApiKeyPolicyProvider>();

        var options = new ApiKeyAuthenticationOptions();
        configureOptions?.Invoke(options);

        return services.AddAuthentication(options.SchemeName)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                options.SchemeName,
                opt =>
                {
                    opt.SchemeName = options.SchemeName;
                    opt.HeaderName = options.HeaderName;
                    opt.QueryParameterName = options.QueryParameterName;
                    opt.ApiKeyIdClaimType = options.ApiKeyIdClaimType;
                    opt.ApiKeyClaimType = options.ApiKeyClaimType;
                });
    }

    public static AuthenticationBuilder AddApiKey(
        this AuthenticationBuilder builder,
        Action<ApiKeyAuthenticationOptions>? configureOptions = null)
    {
        builder.Services.Configure(configureOptions ?? (_ => { }));

        builder.Services.AddSingleton<IAuthorizationHandler, ApiKeyClaimsAuthorizationHandler>();

        builder.Services.AddSingleton<IAuthorizationPolicyProvider, ApiKeyPolicyProvider>();

        var options = new ApiKeyAuthenticationOptions();
        configureOptions?.Invoke(options);

        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            options.SchemeName,
            opt =>
            {
                opt.SchemeName = options.SchemeName;
                opt.HeaderName = options.HeaderName;
                opt.QueryParameterName = options.QueryParameterName;
                opt.ApiKeyIdClaimType = options.ApiKeyIdClaimType;
                opt.ApiKeyClaimType = options.ApiKeyClaimType;
            });
    }
}