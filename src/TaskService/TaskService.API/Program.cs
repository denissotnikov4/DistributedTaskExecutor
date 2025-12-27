using ApiKeys.Client.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using TaskService.Api.DI;
using TaskService.Dal.Data;

namespace TaskService.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/taskservice-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(configure =>
        {
            configure.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Task Management API",
                Version = "v1",
                Description = "API для управления распределенными задачами с TTL"
            });
            configure.AddApiKeySecurity();
        });

        new MainDiModule().RegisterIn(builder.Services, builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAll",
                policy => { policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            dbContext.Database.Migrate();
        }

        Log.Information("Task Service API started.");

        app.Run();
    }
}