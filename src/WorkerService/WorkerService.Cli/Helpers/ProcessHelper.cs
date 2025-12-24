using System.Diagnostics;
using WorkerService.Cli.Models;

namespace WorkerService.Cli.Helpers;

public static class ProcessHelper
{
    public static async Task<RunProcessResult> RunProcessAsync(
        string fileName,
        string arguments,
        string? stdin = null,
        TimeSpan? timeout = null)
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

        try
        {
            await process.WaitForExitAsync(
                timeout is null ? CancellationToken.None : new CancellationTokenSource(timeout.Value).Token);
        }
        catch (Exception exception)
        {
            SafeKillProcess(process);

            var errorMessage = exception is OperationCanceledException
                ? "Process exceeded maximum execution time."
                : $"Unknown exception occured. Error message: {exception.Message}.";

            return new RunProcessResult
            {
                Stdout = null,
                Stderr = errorMessage,
                ExitCode = -1
            };
        }

        return new RunProcessResult
        {
            Stdout = await process.StandardOutput.ReadToEndAsync(),
            Stderr = await process.StandardError.ReadToEndAsync(),
            ExitCode = process.ExitCode
        };
    }

    private static void SafeKillProcess(Process process)
    {
        try
        {
            process.Kill(true);
        }
        catch
        {
            // ignored
        }
    }
}