using System.Globalization;
using System.Text;
using WorkerService.Cli.Models;

namespace WorkerService.Cli.Helpers;

public static class CommandsHelper
{
    public static async Task<RunProcessResult> DockerBuild(
        string imageName,
        string projectPath,
        bool quiet,
        Dictionary<string, string>? buildArgs = null)
    {
        var arguments = new StringBuilder($"build -t {imageName}");

        if (quiet)
        {
            arguments.Append(" --quiet");
        }

        if (buildArgs != null)
        {
            foreach (var arg in buildArgs)
            {
                arguments.Append($" --build-arg {arg.Key}={arg.Value}");
            }
        }

        arguments.Append($" \"{projectPath}\"");

        return await ProcessHelper.RunProcessAsync("docker", arguments.ToString());
    }

    public static async Task<RunProcessResult> DockerRun(
        string imageName,
        string? input,
        double cpuLimit,
        int memoryMbLimit,
        int pidLimit,
        TimeSpan? timeout = null)
    {
        var arguments = new StringBuilder("run");

        if (!string.IsNullOrEmpty(input))
        {
            arguments.Append(" -i");
        }

        arguments.Append(" --rm");
        arguments.Append($" --cpus {cpuLimit.ToString(CultureInfo.InvariantCulture)}");
        arguments.Append($" --memory {memoryMbLimit}m");
        arguments.Append(" --network none");
        arguments.Append(" --read-only");
        arguments.Append(" --security-opt no-new-privileges");
        arguments.Append($" --pids-limit {pidLimit}");
        arguments.Append($" {imageName}");

        return await ProcessHelper.RunProcessAsync("docker", arguments.ToString(), input, timeout);
    }

    public static async Task<RunProcessResult> DockerRmi(string imageName)
    {
        return await ProcessHelper.RunProcessAsync("docker", $"rmi -f {imageName}");
    }
}