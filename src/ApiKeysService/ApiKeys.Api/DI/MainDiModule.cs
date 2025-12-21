using Core.DI;

namespace ApiKeys.Api.DI;

public class MainDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        new RepositoriesDiModule().RegisterIn(services, configuration);
        new ServicesDiModule().RegisterIn(services, configuration);
        new OptionsDiModule().RegisterIn(services, configuration);
        new AuthDiModule().RegisterIn(services, configuration);
        new SwaggerDiModule().RegisterIn(services, configuration);
    }
}

