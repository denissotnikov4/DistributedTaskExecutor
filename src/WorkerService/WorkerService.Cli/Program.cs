using Vostok.Configuration;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Environment;
using Vostok.Configuration.Sources.Json;
using WorkerService.Cli.Services.CodeExecution;
using WorkerService.Cli.Services.CodeExecution.Executors;
using WorkerService.Cli.Services.ProjectCreators;
using WorkerService.Cli.Services.ProjectCreators.Base;
using WorkerService.Cli.Settings;

namespace WorkerService.Cli;

public static class Program
{
    public static async Task Main()
    {
        var settings = GetServiceSettings();

        var codeExecutionService = new CodeExecutionService(
            new IProjectCreator[]
            {
                new CSharpProjectCreator(settings.CodeExecution.CSharp),
                new PythonProjectCreator()
            },
            new DockerExecutor(settings.CodeExecution));

        Console.WriteLine("App initialized.");
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