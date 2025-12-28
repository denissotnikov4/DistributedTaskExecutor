using DistributedTaskExecutor.Core.DI;
using Microsoft.EntityFrameworkCore;
using TaskService.Dal.Data;
using TaskService.Dal.Repositories;

namespace TaskService.Api.DI;

public class RepositoriesDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaskDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITaskRepository, TaskRepository>();
    }
}