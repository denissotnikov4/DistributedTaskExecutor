using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Database;

public static class DependencyInjection
{
    public static void AddNpgsqlDbContext<TContext>(this IServiceCollection services, IConfiguration config)
        where TContext : DbContext
    {
        var dbOptions = new DatabaseOptions();
        config.GetSection("DatabaseOptions").Bind(dbOptions);
        dbOptions.Host = Environment.GetEnvironmentVariable("DB_CONTAINER") ?? "localhost";
        dbOptions.Port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
        dbOptions.User = Environment.GetEnvironmentVariable("DATABASE_USER") ?? "postgres";
        dbOptions.Password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "postgres";

        services.Configure<DatabaseOptions>(options =>
        {
            options.Name = dbOptions.Name;
            options.Host = dbOptions.Host;
            options.Port = dbOptions.Port;
            options.User = dbOptions.User;
            options.Password = dbOptions.Password;
        });

        services.AddDbContext<TContext>(options => { options.UseNpgsql(dbOptions.GetConnectionString()); });
    }
}