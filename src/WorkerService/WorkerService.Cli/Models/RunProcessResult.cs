namespace WorkerService.Cli.Models;

public readonly struct RunProcessResult
{
    public string? Stdout { get; init; }

    public string? Stderr { get; init; }

    public int ExitCode { get; init; }

    public void ThrowIfFailed(Func<Exception> getException, int successStatusCode = 0)
    {
        if (this.ExitCode != successStatusCode)
        {
            throw getException();
        }
    }
}