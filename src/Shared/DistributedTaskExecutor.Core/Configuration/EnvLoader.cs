using DotNetEnv;

namespace DistributedTaskExecutor.Core.Configuration;

public static class EnvLoader
{
    public static void LoadEnvFile(string? envFilePath = null)
    {
        if (string.IsNullOrEmpty(envFilePath))
        {
            var currentDir = Directory.GetCurrentDirectory();
            var envFile = FindEnvFile(currentDir);

            if (envFile != null)
            {
                Env.Load(envFile);
            }
        }
        else
        {
            Env.Load(envFilePath);
        }
    }

    private static string? FindEnvFile(string startDirectory)
    {
        var currentDir = new DirectoryInfo(startDirectory);

        while (currentDir != null)
        {
            var envFile = Path.Combine(currentDir.FullName, ".env");
            if (File.Exists(envFile))
            {
                return envFile;
            }

            currentDir = currentDir.Parent;
        }

        return null;
    }
}