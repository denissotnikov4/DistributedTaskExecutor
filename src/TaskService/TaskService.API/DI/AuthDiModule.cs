using ApiKeys.Client;
using ApiKeys.Client.Extensions;
using Core.DI;

namespace TaskService.Api.DI;

public class AuthDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        var apiKeysServiceUrl = configuration["ApiKeysService:BaseUrl"]!;

        services.AddApiKeysClient(apiKeysServiceUrl);
        services.AddApiKeyAuthentication();

        services.AddAuthorization();
    }
}