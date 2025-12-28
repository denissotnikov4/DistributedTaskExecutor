using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedTaskExecutor.Core.DI;

public interface IDiModule
{
    void RegisterIn(IServiceCollection services, IConfiguration configuration);
}