using WorkerService.Cli.Models;
using ExecutionContext = WorkerService.Cli.Models.ExecutionContext;

namespace WorkerService.Cli.Services.CodeExecution;

public interface ICodeExecutionService
{
    Task<ExecutionResult> ExecuteAsync(ExecutionContext context);
}