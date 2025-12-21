using WorkerService.Cli.Exceptions;
using WorkerService.Cli.Helpers;
using WorkerService.Cli.Models;
using WorkerService.Cli.Services.CodeExecutors.Base;
using WorkerService.Cli.Services.ProjectCreators.Base;
using ExecutionContext = WorkerService.Cli.Models.ExecutionContext;

namespace WorkerService.Cli.Services.CodeExecutors;

public class DockerExecutor : ICodeExecutor
{
    private readonly ICollection<IProjectCreator> projectCreators;

    public DockerExecutor(ICollection<IProjectCreator> projectCreators)
    {
        this.projectCreators = projectCreators ?? throw new ArgumentNullException(nameof(projectCreators));
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionContext context)
    {
        string? projectPath = null;

        try
        {
            projectPath = await this.CreateProject(context);

            var buildResult = await CommandsHelper.DockerBuild(context.Name, projectPath);

            buildResult.ThrowIfFailed(() => new DockerBuildException(context.Name, buildResult.Stderr!));

            var runResult = await CommandsHelper.DockerRun(context.Name, context.Input);

            return new ExecutionResult { Output = runResult.Stdout, ErrorMessage = runResult.Stderr };
        }
        finally
        {
            await CommandsHelper.DockerRmi(context.Name);

            DirectoryHelper.DeleteIfExists(projectPath!);
        }
    }

    private async Task<string> CreateProject(ExecutionContext context)
    {
        var projectCreator = this.projectCreators.FirstOrDefault(creator => creator.Accept(context.Language));

        if (projectCreator == null)
        {
            throw new NotSupportedException($"Programming language \"{context.Language.ToString()}\" is not supported.");
        }

        var projectPath = await projectCreator.CreateAsync(context.Name, context.Code);

        var dockerfileName = ResourcesHelper.GetDockerfileNameByProgrammingLanguage(context.Language);

        await File.WriteAllTextAsync(
            Path.Combine(projectPath, Constants.DockerFile),
            await ResourcesHelper.GetResourceAsync(dockerfileName));

        return projectPath;
    }
}