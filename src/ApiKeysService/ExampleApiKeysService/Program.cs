using ApiKeys.Client;
using ApiKeys.Client.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Example ApiKeys Service API",
        Version = "v1",
        Description = "Пример использования ApiKeys.Client для аутентификации по API-ключам"
    });
    options.AddApiKeySecurity();
});

var apiKeysServiceUrl = builder.Configuration["ApiKeysService:BaseUrl"]!;
builder.Services.AddApiKeysClient(apiKeysServiceUrl);

builder.Services.AddApiKeyAuthentication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
