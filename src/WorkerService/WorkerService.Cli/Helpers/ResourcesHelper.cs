using System.Collections.Concurrent;
using System.Reflection;
using TaskService.Client.Models.Tasks;
using WorkerService.Cli.Models;

namespace WorkerService.Cli.Helpers;

public static class ResourcesHelper
{
    private static readonly ConcurrentDictionary<string, string> Cache = new();

    public static async Task<string> GetResourceAsync(string resourceName, CancellationToken cancelToken = default)
    {
        if (Cache.TryGetValue(resourceName, out var value))
        {
            return value;
        }

        var assembly = Assembly.GetExecutingAssembly();

        var fullResourceName = assembly
            .GetManifestResourceNames()
            .Single(name => name.EndsWith(resourceName));

        await using var stream = assembly.GetManifestResourceStream(fullResourceName)!;
        using var reader = new StreamReader(stream);

        var resourceValue = await reader.ReadToEndAsync(cancelToken);

        Cache[resourceName] = resourceValue;

        return resourceValue;
    }

    public static string GetDockerfileNameByProgrammingLanguage(ProgrammingLanguage language)
    {
        return $"{language.ToString().ToLower()}.{Constants.DockerfileName}";
    }
}