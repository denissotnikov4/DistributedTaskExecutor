using Core.DI;
using TaskService.Logic.Validators;

namespace TaskService.Api.DI;

public class ValidatorsDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<TaskCreateRequestValidator>();
    }
}