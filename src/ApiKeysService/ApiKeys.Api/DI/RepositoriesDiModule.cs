using ApiKeys.Dal.Data;
using ApiKeys.Dal.Repositories;
using DistributedTaskExecutor.Core.Database;
using DistributedTaskExecutor.Core.DI;

namespace ApiKeys.Api.DI;

public class RepositoriesDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNpgsqlDbContext<ApiKeyDbContext>("API_KEYS_CONNECTION_STRING");

        services.AddScoped<IApiKeysRepository, ApiKeysRepository>();
        
        services.AddScoped<IApiKeysUnitOfWork, ApiKeysUnitOfWork>();
    }
}

