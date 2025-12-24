using WorkerService.Cli.Helpers;
using WorkerService.Cli.Models;
using WorkerService.Cli.Services.CodeExecution.Executors.Base;
using WorkerService.Cli.Services.ProjectCreators.Base;
using ExecutionContext = WorkerService.Cli.Models.ExecutionContext;

namespace WorkerService.Cli.Services.CodeExecution;

public class CodeExecutionService : ICodeExecutionService
{
    private readonly ICollection<IProjectCreator> projectCreators;
    private readonly ICodeExecutor codeExecutor;

    public CodeExecutionService(
        ICollection<IProjectCreator> projectCreators,
        ICodeExecutor codeExecutor)
    {
        this.projectCreators = projectCreators ?? throw new ArgumentNullException(nameof(projectCreators));
        this.codeExecutor = codeExecutor ?? throw new ArgumentNullException(nameof(codeExecutor));
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionContext context)
    {
        var projectCreator = this.projectCreators.FirstOrDefault(creator => creator.Accept(context.Language));

        if (projectCreator == null)
        {
            throw new NotSupportedException($"Programming language \"{context.Language.ToString()}\" is not supported.");
        }

        var projectPath = await projectCreator.CreateAsync(context.Name, context.Code);

        try
        {
            return await this.codeExecutor.ExecuteAsync(context.Language, context.Input, projectPath, context.Name);
        }
        catch
        {
            DirectoryHelper.DeleteIfExists(projectPath);
            throw;
        }
    }
}