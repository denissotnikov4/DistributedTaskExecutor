using ApiKeys.Api.Configuration;
using ApiKeys.Api.DI;
using Core.Auth;
using Core.Configuration;
using Core.Swagger;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

EnvLoader.LoadEnvFile();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services
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

builder.Services.AddJwtAuth(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.ManageApiKey, policy => 
        policy.RequireClaim("claim", Claims.ManageApiKey));
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

Log.Information("API Keys Service started.");

app.Run();
