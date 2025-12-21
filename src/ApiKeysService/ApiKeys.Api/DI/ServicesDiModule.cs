using ApiKeys.Logic.Services;
using Core.DI;

namespace ApiKeys.Api.DI;

public class ServicesDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IApiKeysService, ApiKeysService>();
        services.AddScoped<IApiKeyGenerator, ApiKeyGenerator>();
    }
}

