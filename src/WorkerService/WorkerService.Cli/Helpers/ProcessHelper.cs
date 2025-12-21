using System.Diagnostics;
using WorkerService.Cli.Models;

namespace WorkerService.Cli.Helpers;

public static class ProcessHelper
{
    public static async Task<RunProcessResult> RunProcessAsync(
        string fileName,
        string arguments,
        string? stdin = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = stdin != null,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)!;

        if (!string.IsNullOrEmpty(stdin))
        {
            await process.StandardInput.WriteAsync(stdin);
            await process.StandardInput.FlushAsync();

            process.StandardInput.Close();
        }

        await process.WaitForExitAsync();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        return new RunProcessResult
        {
            Stdout = stdout,
            Stderr = stderr,
            ExitCode = process.ExitCode
        };
    }
}