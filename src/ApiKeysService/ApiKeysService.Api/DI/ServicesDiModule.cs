using ApiKeysService.Logic.Services;
using Core.DI;

namespace ApiKeysService.Api.DI;

public class ServicesDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IApiKeysService, Logic.Services.ApiKeysService>();
    }
}

