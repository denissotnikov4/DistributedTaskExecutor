using Core.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Database;

public static class DatabaseDiExtensions
{
    public static IServiceCollection AddNpgsqlDbContext<TContext>(
        this IServiceCollection services, string connectionStringEnvVar) where TContext : DbContext
    {
        var connectionString = EnvHelper.Require(connectionStringEnvVar);

        services.Configure<DatabaseOptions>(options =>
        {
            options.ConnectionString = connectionString;
        });

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        return services;
    }
}