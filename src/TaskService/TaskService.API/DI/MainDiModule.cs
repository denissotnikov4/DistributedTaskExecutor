using Core.DI;

namespace TaskService.Api.DI;

public class MainDiModule : IDiModule
{
    private readonly ICollection<IDiModule> modules = new List<IDiModule>
    {
        new LogicDiModule(),
        new RabbitDiModule(),
        new RepositoriesDiModule(),
        new ValidatorsDiModule()
    };

    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        foreach (var module in this.modules)
        {
            module.RegisterIn(services, configuration);
        }
    }
}