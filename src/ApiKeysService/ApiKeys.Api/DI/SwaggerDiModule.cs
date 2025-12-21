using Core.DI;
using Core.Swagger;
using Microsoft.OpenApi.Models;

namespace ApiKeys.Api.DI;

public class SwaggerDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Keys Service API",
                    Version = "v1",
                    Description = "API для управления API-ключами с поддержкой claims"
                });
                options.AddJwtSecurity();
            });
    }
}