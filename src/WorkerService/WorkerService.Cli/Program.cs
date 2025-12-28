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

namespace WorkerService.Cli;

public static class Program
{
    public static async Task Main()
    {
        await GetWorkerService().RunAsync();
    }

    private static IWorkerService GetWorkerService()
    {
        const string logFileName = "log.txt";

        var loggerFactory = new SerilogLoggerFactory(
            new LoggerConfiguration().WriteTo.Console().WriteTo.File(logFileName).CreateLogger());

        var workerServiceSettings = GetServiceSettings();

        var codeExecutionService = new CodeExecutionService(
            new ProjectCreationService(),
            new DockerExecutor(
                new DefaultDockerBuildArgsFactory(),
                workerServiceSettings.CodeExecution));

        var rabbitMessageQueue = new RabbitMessageQueue<Guid>(
            workerServiceSettings.RabbitSettings.ToCoreSettings(),
            new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            },
            loggerFactory.CreateLogger<RabbitMessageQueue<Guid>>());

        var taskServiceClient = new TaskServiceClient(
            workerServiceSettings.TaskServiceApiUrl,
            workerServiceSettings.ApiKey,
            loggerFactory.CreateLogger<TaskServiceClient>());

        return new Services.WorkerService(
            codeExecutionService,
            rabbitMessageQueue,
            taskServiceClient,
            loggerFactory.CreateLogger<Services.WorkerService>());
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