using ApiKeys.Dal.Data;
using ApiKeys.Dal.Repositories;
using Core.DI;
using Microsoft.EntityFrameworkCore;

namespace ApiKeys.Api.DI;

public class RepositoriesDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApiKeyDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly("ApiKeys.Dal")));

        services.AddScoped<IApiKeysRepository, ApiKeysRepository>();
        
        services.AddScoped<IApiKeysUnitOfWork, ApiKeysUnitOfWork>();
    }
}

