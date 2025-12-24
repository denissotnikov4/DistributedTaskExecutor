using TaskService.Client.Models.Tasks;
using WorkerService.Cli.Exceptions;
using WorkerService.Cli.Helpers;
using WorkerService.Cli.Models;
using WorkerService.Cli.Services.CodeExecution.Executors.Base;
using WorkerService.Cli.Settings.CodeExecution;

namespace WorkerService.Cli.Services.CodeExecution.Executors;

public class DockerExecutor : ICodeExecutor
{
    private readonly CodeExecutionSettings executionSettings;

    public DockerExecutor(CodeExecutionSettings executionSettings)
    {
        this.executionSettings = executionSettings ?? throw new ArgumentNullException(nameof(executionSettings));
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

            var buildResult = await CommandsHelper.DockerBuild(
                contextName,
                projectPath,
                true,
                this.GetLanguageBuildArgs(language));

            buildResult.ThrowIfFailed(() => new DockerBuildException(contextName, buildResult.Stderr!));

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
            Path.Combine(projectPath, Constants.DockerFile),
            await ResourcesHelper.GetResourceAsync(dockerfileName));
    }

    private Dictionary<string, string> GetLanguageBuildArgs(ProgrammingLanguage language)
    {
        return language switch
        {
            ProgrammingLanguage.CSharp => new Dictionary<string, string>
            {
                ["SDK_IMAGE"] = this.executionSettings.CSharp.DotnetSdkImageName,
                ["RUNTIME_IMAGE"] = this.executionSettings.CSharp.DotnetRuntimeImageName
            },

            ProgrammingLanguage.Python => new Dictionary<string, string>
            {
                ["LANGUAGE_IMAGE"] = this.executionSettings.Python.ImageName
            },

            _ => throw new ArgumentOutOfRangeException(
                nameof(language),
                "No docker build arguments defined for specified programming language.")
        };
    }
}