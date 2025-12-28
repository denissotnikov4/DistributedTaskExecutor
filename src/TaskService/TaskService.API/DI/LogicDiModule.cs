using DistributedTaskExecutor.Core.DI;
using TaskService.Logic.Services.Tasks;

namespace TaskService.Api.DI;

public class LogicDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<TaskExpirationService>();
        services.AddScoped<ITaskService, Logic.Services.Tasks.TaskService>();
    }
}