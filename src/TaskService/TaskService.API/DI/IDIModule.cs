namespace TaskService.Api.DI;

public interface IDiModule
{
    void RegisterIn(IServiceCollection services, IConfiguration configuration);
}