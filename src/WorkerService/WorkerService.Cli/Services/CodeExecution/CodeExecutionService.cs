using WorkerService.Cli.Helpers;
using WorkerService.Cli.Models;
using WorkerService.Cli.Services.CodeExecution.Executors.Base;
using WorkerService.Cli.Services.ProjectCreation;
using ExecutionContext = WorkerService.Cli.Models.ExecutionContext;

namespace WorkerService.Cli.Services.CodeExecution;

public class CodeExecutionService : ICodeExecutionService
{
    private readonly IProjectCreationService projectCreationService;
    private readonly ICodeExecutor codeExecutor;

    public CodeExecutionService(
        IProjectCreationService projectCreationService,
        ICodeExecutor codeExecutor)
    {
        this.projectCreationService = projectCreationService ?? throw new ArgumentNullException(nameof(projectCreationService));
        this.codeExecutor = codeExecutor ?? throw new ArgumentNullException(nameof(codeExecutor));
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionContext context)
    {
        var projectPath = await this.projectCreationService.CreateAsync(context.Name, context.Code, context.Language);

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