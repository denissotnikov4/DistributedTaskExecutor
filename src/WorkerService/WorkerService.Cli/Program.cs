using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DistributedTaskExecutor.Core.RabbitMQ;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using TaskService.Client;
using Vostok.Configuration;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Environment;
using Vostok.Configuration.Sources.Json;
using WorkerService.Cli.Services;
using WorkerService.Cli.Services.CodeExecution;
using WorkerService.Cli.Services.CodeExecution.Executors.Docker;
using WorkerService.Cli.Services.ProjectCreation;
using WorkerService.Cli.Settings;
using WorkerService.Cli.Settings.CodeExecution;

namespace WorkerService.Cli;

public static class Program
{
    public static async Task Main()
    {
        await GetWorkerService().RunAsync();
    }

    private static IWorkerService GetWorkerService()
    {
        var loggerFactory = GetLoggerFactory();
        var workerServiceSettings = GetServiceSettings();

        return new Services.WorkerService(
            GetCodeExecutionService(workerServiceSettings.CodeExecution, loggerFactory),
            GetRabbitMessageQueue(workerServiceSettings.Rabbit.ToCoreSettings(), loggerFactory),
            GetTaskServiceClient(workerServiceSettings.TaskServiceApiUrl, workerServiceSettings.ApiKey, loggerFactory),
            loggerFactory.CreateLogger<Services.WorkerService>());
    }

    private static ICodeExecutionService GetCodeExecutionService(CodeExecutionSettings settings, ILoggerFactory loggerFactory)
    {
        return new CodeExecutionService(
            new ProjectCreationService(loggerFactory.CreateLogger<ProjectCreationService>()),
            new DockerExecutor(
                new DefaultDockerBuildArgsFactory(),
                settings,
                loggerFactory.CreateLogger<DockerExecutor>()));
    }

    private static IRabbitMessageQueue<Guid> GetRabbitMessageQueue(
        RabbitMqSettings rabbitSettings,
        ILoggerFactory loggerFactory)
    {
        return new RabbitMessageQueue<Guid>(
            rabbitSettings,
            new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            },
            loggerFactory.CreateLogger<RabbitMessageQueue<Guid>>());
    }

    private static ITaskServiceClient GetTaskServiceClient(string url, string apiKey, ILoggerFactory loggerFactory)
    {
        return new TaskServiceClient(url, apiKey, loggerFactory.CreateLogger<TaskServiceClient>());
    }

    private static ILoggerFactory GetLoggerFactory()
    {
        const string logFileName = "log.txt";

        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logFileName);

        return new SerilogLoggerFactory(loggerConfiguration.CreateLogger());
    }

    private static WorkerServiceSettings GetServiceSettings()
    {
        const string settingsFilename = "settings.json";

        return new ConfigurationProvider().Get<WorkerServiceSettings>(
            new CombinedSource(
                new JsonFileSource(Path.Combine(Directory.GetCurrentDirectory(), settingsFilename)),
                new EnvironmentVariablesSource()));
    }
}