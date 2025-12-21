using ApiKeys.Client;
using ApiKeys.Client.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
