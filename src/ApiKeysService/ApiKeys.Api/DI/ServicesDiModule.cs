using ApiKeys.Logic.Services.ApiKeys;
using ApiKeys.Logic.Services.Auth;
using Core.DI;

namespace ApiKeys.Api.DI;

public class ServicesDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IApiKeysService, ApiKeysService>();
        services.AddScoped<IApiKeyGenerator, ApiKeyGenerator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<ITokenProvider, JwtTokenProvider>();
    }
}