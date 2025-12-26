using System.Text;
using ApiKeys.Client;
using ApiKeys.Client.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TaskService.Api.DI;
using TaskService.Dal.Data;

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
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TaskService";

var apiKeysServiceUrl = builder.Configuration["ApiKeysService:BaseUrl"]!;

builder.Services.AddApiKeysClient(apiKeysServiceUrl);
builder.Services.AddApiKeyAuthentication();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    })
    .AddApiKey();

builder.Services.AddAuthorization();

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