using TaskService.Client.Models.Tasks;
using WorkerService.Cli.Models;

namespace WorkerService.Cli.Services.CodeExecution.Executors.Base;

public interface ICodeExecutor
{
    Task<ExecutionResult> ExecuteAsync(
        ProgrammingLanguage language,
        string? input,
        string projectPath,
        string contextName);
}