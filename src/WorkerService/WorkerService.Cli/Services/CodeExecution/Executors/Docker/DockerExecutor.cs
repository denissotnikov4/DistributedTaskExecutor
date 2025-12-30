using Microsoft.Extensions.Logging;
using TaskService.Client.Models.Tasks;
using WorkerService.Cli.Exceptions;
using WorkerService.Cli.Helpers;
using WorkerService.Cli.Models;
using WorkerService.Cli.Services.CodeExecution.Executors.Base;
using WorkerService.Cli.Settings.CodeExecution;

namespace WorkerService.Cli.Services.CodeExecution.Executors.Docker;

public class DockerExecutor : ICodeExecutor
{
    private readonly IDockerBuildArgsFactory dockerBuildArgsFactory;
    private readonly CodeExecutionSettings executionSettings;
    private readonly ILogger<DockerExecutor> logger;

    public DockerExecutor(
        IDockerBuildArgsFactory dockerBuildArgsFactory,
        CodeExecutionSettings executionSettings,
        ILogger<DockerExecutor> logger)
    {
        this.dockerBuildArgsFactory = dockerBuildArgsFactory ?? throw new ArgumentNullException(nameof(dockerBuildArgsFactory));
        this.executionSettings = executionSettings ?? throw new ArgumentNullException(nameof(executionSettings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExecutionResult> ExecuteAsync(
        ProgrammingLanguage language,
        string? input,
        string projectPath,
        string contextName)
    {
        try
        {
            await AddDockerfileToProject(projectPath, language);

            this.logger.LogInformation(
                "\"{language}\" dockerfile successfully added to project \"{projectPath}\".",
                language,
                projectPath);

            var buildResult = await CommandsHelper.DockerBuild(
                contextName,
                projectPath,
                true,
                this.GetLanguageBuildArgs(language));

            buildResult.ThrowIfFailed(() => new DockerBuildException(contextName, buildResult.Stderr!));

            this.logger.LogInformation("Docker image build succeeded for project \"{projectPath}\".", projectPath);

            var runResult = await CommandsHelper.DockerRun(
                contextName,
                input,
                this.executionSettings.Docker.CpuLimit!.Value,
                this.executionSettings.Docker.MemoryMbLimit!.Value,
                this.executionSettings.Docker.PidLimit!.Value,
                this.executionSettings.Timeout);

            return new ExecutionResult
            {
                Output = runResult.Stdout,
                ErrorMessage = runResult.Stderr
            };
        }
        finally
        {
            await CommandsHelper.DockerRmi(contextName);
        }
    }

    private static async Task AddDockerfileToProject(string projectPath, ProgrammingLanguage language)
    {
        var dockerfileName = ResourcesHelper.GetDockerfileNameByProgrammingLanguage(language);

        await File.WriteAllTextAsync(
            Path.Combine(projectPath, Constants.DockerfileName),
            await ResourcesHelper.GetResourceAsync(dockerfileName));
    }

    private Dictionary<string, string> GetLanguageBuildArgs(ProgrammingLanguage language)
    {
        return language switch
        {
            ProgrammingLanguage.CSharp => this.dockerBuildArgsFactory.GetCSharpBuildArgs(this.executionSettings.CSharp),
            ProgrammingLanguage.Python => this.dockerBuildArgsFactory.GetPythonBuildArgs(this.executionSettings.Python),
            _ => throw new ArgumentOutOfRangeException(
                nameof(language),
                "No docker build arguments defined for specified programming language.")
        };
    }
}