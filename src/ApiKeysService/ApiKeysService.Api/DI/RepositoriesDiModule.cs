using ApiKeysService.Dal.Data;
using ApiKeysService.Dal.Repositories;
using Core.DI;
using Microsoft.EntityFrameworkCore;

namespace ApiKeysService.Api.DI;

public class RepositoriesDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApiKeyDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly("ApiKeysService.Dal")));

        services.AddScoped<IApiKeysRepository, ApiKeysRepository>();
    }
}

