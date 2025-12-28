using TaskService.Client.Models.Tasks;

namespace WorkerService.Cli.Models;

public class ExecutionContext
{
    public string Name { get; init; }

    public string Code { get; init; }

    public ProgrammingLanguage Language { get; init; }

    public string? Input { get; init; }
}