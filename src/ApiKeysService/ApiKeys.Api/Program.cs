using ApiKeys.Api.DI;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(configure =>
{
    configure.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Keys Service API",
        Version = "v1",
        Description = "API для управления API-ключами с поддержкой claims"
    });
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
app.UseAuthorization();

app.MapControllers();

Log.Information("API Keys Service started.");

app.Run();
