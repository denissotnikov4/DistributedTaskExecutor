using Serilog;
using TaskExecutor.Infrastructure;
using TaskExecutor.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

Log.Information("Task Executor Worker started");

host.Run();