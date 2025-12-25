using Vostok.Configuration;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Environment;
using Vostok.Configuration.Sources.Json;
using WorkerService.Cli.Settings;

namespace WorkerService.Cli;

public static class Program
{
    public static async Task Main()
    {
        throw new NotImplementedException();
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