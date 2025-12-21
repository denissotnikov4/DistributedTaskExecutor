namespace Core.Configuration;

public static class EnvHelper
{
    public static string? Get(string name, string? defaultValue = null)
    {
        return Environment.GetEnvironmentVariable(name) ?? defaultValue;
    }

    public static string Require(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Required environment variable '{name}' is not set")
            : value;
    }
}