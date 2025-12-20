using Microsoft.Extensions.DependencyInjection;

namespace ApiKeysService.Client;

public static class ApiKeysClientExtensions
{
    public static IServiceCollection AddApiKeysClient(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient<IApiKeysClient, ApiKeysClient>(client => { client.BaseAddress = new Uri(baseUrl); });

        return services;
    }
}