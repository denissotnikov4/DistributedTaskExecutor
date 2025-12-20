namespace WorkerService.Cli.Services.CodeRunners.Models;

public class ExecutionContext
{
    public string Id { get; set; }

    public string Language { get; set; }

    public string Code { get; init; }

    public string? StdIn { get; init; }
}