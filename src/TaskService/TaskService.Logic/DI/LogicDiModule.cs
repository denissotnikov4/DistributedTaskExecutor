using Core.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskService.Logic.Services.Tasks;

namespace TaskService.Logic.DI;

public class LogicDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<TaskExpirationService>();
        services.AddScoped<ITaskService, Logic.Services.Tasks.TaskService>();
    }
}