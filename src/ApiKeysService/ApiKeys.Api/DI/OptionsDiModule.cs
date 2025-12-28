using ApiKeys.Logic.Configuration;
using DistributedTaskExecutor.Core.Configuration;
using DistributedTaskExecutor.Core.DI;

namespace ApiKeys.Api.DI;

public class OptionsDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdminUserOption>(options =>
        {
            options.Username = EnvHelper.Require("ADMIN_USERNAME");
            options.Password = EnvHelper.Require("ADMIN_PASSWORD");
            options.Claims = EnvHelper.Require("ADMIN_CLAIMS").Split(';');
        });
    }
}