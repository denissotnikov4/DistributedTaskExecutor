using System.Globalization;
using System.Text;
using WorkerService.Cli.Models;

namespace WorkerService.Cli.Helpers;

public static class CommandsHelper
{
    public static async Task<RunProcessResult> DotnetNew(
        string projectPath,
        string projectName = Constants.DefaultProjectName,
        string framework = "net8.0",
        string languageVersion = "12.0")
    {
        return await ProcessHelper.RunProcessAsync(
            "dotnet",
            $"new console --framework {framework} --langVersion {languageVersion} -o \"{projectPath}\" -n {projectName}");
    }

    public static async Task<RunProcessResult> DockerBuild(string imageName, string projectPath, bool quiet = true)
    {
        return await ProcessHelper.RunProcessAsync(
            "docker",
            $"build -t {imageName} {(quiet ? "--quiet " : string.Empty)}\"{projectPath}\"");
    }

    public static async Task<RunProcessResult> DockerRun(
        string imageName,
        string? input,
        int memoryMb = 256,
        double cpuLimit = 0.5,
        int pidLimit = 100)
    {
        var stringBuilder = new StringBuilder("run");

        if (!string.IsNullOrEmpty(input))
        {
            stringBuilder.Append(" -i");
        }

        stringBuilder.Append(" --rm");
        stringBuilder.Append($" --memory {memoryMb}m");
        stringBuilder.Append($" --cpus {cpuLimit.ToString(CultureInfo.InvariantCulture)}");
        stringBuilder.Append(" --network none");
        stringBuilder.Append(" --read-only");
        stringBuilder.Append(" --security-opt no-new-privileges");
        stringBuilder.Append($" --pids-limit {pidLimit}");
        stringBuilder.Append($" {imageName}");

        return await ProcessHelper.RunProcessAsync("docker", stringBuilder.ToString(), input);
    }

    public static async Task<RunProcessResult> DockerRmi(string imageName)
    {
        return await ProcessHelper.RunProcessAsync("docker", $"rmi -f {imageName}");
    }
}