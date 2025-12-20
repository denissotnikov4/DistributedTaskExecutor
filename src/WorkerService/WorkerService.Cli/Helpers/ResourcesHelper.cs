using System.Reflection;

namespace WorkerService.Cli.Helpers;

public static class ResourcesHelper
{
    private static readonly Dictionary<string, string> Cache = new();

    public static string GetResource(string resourceName)
    {
        if (Cache.TryGetValue(resourceName, out var value))
        {
            return value;
        }

        var assembly = Assembly.GetExecutingAssembly();

        var fullResourceName = assembly
            .GetManifestResourceNames()
            .Single(name => name.EndsWith(resourceName));

        using var stream = assembly.GetManifestResourceStream(fullResourceName)!;
        using var reader = new StreamReader(stream);

        var resourceValue = reader.ReadToEnd();

        Cache[resourceName] = resourceValue;

        return resourceValue;
    }
}