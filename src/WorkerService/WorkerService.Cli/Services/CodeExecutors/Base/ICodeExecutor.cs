using WorkerService.Cli.Models;
using ExecutionContext = WorkerService.Cli.Models.ExecutionContext;

namespace WorkerService.Cli.Services.CodeExecutors.Base;

/// <summary>
/// Класс, выполняющий пользовательский код.
/// </summary>
public interface ICodeExecutor
{
    /// <summary>
    /// Выполнить код.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task<ExecutionResult> ExecuteAsync(ExecutionContext context);
}