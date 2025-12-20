using System.Diagnostics;

namespace WorkerService.Cli.Helpers;

public static class ProcessHelper
{
    public static async Task<(string? stdout, string? stderr, int exitCode)> RunProcessAsync(
        string fileName,
        string arguments,
        string? input = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = input != null,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)!;

        if (!string.IsNullOrEmpty(input))
        {
            await process.StandardInput.WriteAsync(input);
            await process.StandardInput.FlushAsync();

            process.StandardInput.Close();
        }

        await process.WaitForExitAsync();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        return (stdout, stderr, process.ExitCode);
    }
}