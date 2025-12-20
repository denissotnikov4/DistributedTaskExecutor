using System.Text;
using WorkerService.Cli.Helpers;
using WorkerService.Cli.Services.CodeRunners.Base;
using WorkerService.Cli.Services.CodeRunners.Models;
using WorkerService.Cli.Services.ProjectCreators.Base;
using ExecutionContext = WorkerService.Cli.Services.CodeRunners.Models.ExecutionContext;

namespace WorkerService.Cli.Services.CodeRunners;

public class DockerRunner : ICodeRunner
{
    private readonly ICollection<IProjectCreator> projectCreators;

    public DockerRunner(ICollection<IProjectCreator> projectCreators)
    {
        this.projectCreators = projectCreators ?? throw new ArgumentNullException(nameof(projectCreators));
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionContext context)
    {
        string? projectPath = null;

        try
        {
            projectPath = await this.CreateProject(context);

            var (a, b, c) = await ProcessHelper.RunProcessAsync("docker", $"build -t {context.Id} \"{projectPath}\"");

            var (result, errorMessage, _) = await ProcessHelper.RunProcessAsync(
                "docker",
                GetRunCommand(context.Id, context.StdIn, 256, 1, 100),
                context.StdIn);

            return new ExecutionResult
            {
                Result = result,
                ErrorMessage = errorMessage
            };
        }
        finally
        {
            await ProcessHelper.RunProcessAsync("docker", $"rmi -f {context.Id}");

            DirectoryHelper.DeleteIfExists(projectPath!);
        }
    }

    private async Task<string> CreateProject(ExecutionContext context)
    {
        var projectCreator = this.projectCreators.FirstOrDefault(creator => creator.Accept(context.Language));

        if (projectCreator == null)
        {
            // TODO: Создать спец. тип исключений.
            throw new Exception("Language not supported.");
        }

        return await projectCreator.CreateAsync(context.Id, context.Code);
    }

    private static string GetRunCommand(string imageTag, string? stdIn, int memoryMB, double cpuLimit, int pidLimit)
    {
        var stringBuilder = new StringBuilder("run");

        if (!string.IsNullOrEmpty(stdIn))
        {
            stringBuilder.Append(" -i");
        }

        stringBuilder.Append(" --rm");
        stringBuilder.Append($" --memory {memoryMB}m");
        stringBuilder.Append($" --cpus {cpuLimit:0.##}");
        stringBuilder.Append(" --network none");
        stringBuilder.Append(" --read-only");
        stringBuilder.Append(" --security-opt no-new-privileges");
        stringBuilder.Append($" --pids-limit {pidLimit}");
        stringBuilder.Append($" {imageTag}");

        return stringBuilder.ToString();
    }
}