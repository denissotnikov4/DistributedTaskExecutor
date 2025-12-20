using WorkerService.Cli.Services.CodeRunners.Models;
using ExecutionContext = WorkerService.Cli.Services.CodeRunners.Models.ExecutionContext;

namespace WorkerService.Cli.Services.CodeRunners.Base;

public interface ICodeRunner
{
    Task<ExecutionResult> ExecuteAsync(ExecutionContext context);
}