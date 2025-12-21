using TaskService.Client.Models.Tasks;

namespace WorkerService.Cli.Models;

public class ExecutionContext
{
    public string Name { get; set; }

    public ProgrammingLanguage Language { get; set; }

    public string Code { get; init; }

    public string? Input { get; init; }
}