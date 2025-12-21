using Core.DI;

namespace ApiKeys.Api.DI;

public class MainDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        new RepositoriesDiModule().RegisterIn(services, configuration);
        new ServicesDiModule().RegisterIn(services, configuration);
    }
}

