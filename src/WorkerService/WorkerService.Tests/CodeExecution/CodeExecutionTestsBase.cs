using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WorkerService.Cli.Services.CodeExecution;
using WorkerService.Cli.Services.CodeExecution.Executors.Docker;
using WorkerService.Cli.Services.ProjectCreation;
using WorkerService.Cli.Settings.CodeExecution;
using ExecutionContext = WorkerService.Cli.Models.ExecutionContext;

namespace WorkerService.Tests.CodeExecution;

/// <summary>
/// Требуется запущенный docker-демон на хосте.
/// </summary>
[TestFixture]
public abstract class CodeExecutionTestsBase
{
    [Test]
    public async Task ExecuteCodeWithInput_Success()
    {
        await ValidateExecute(this.GetContextFor_ExecuteCodeWithInput_Success(), false);
    }

    [Test]
    public async Task ExecuteCodeWithoutInput_Success()
    {
        await ValidateExecute(this.GetContextFor_ExecuteCodeWithoutInput_Success(), false);
    }

    [Test]
    public async Task ExecuteCode_Error()
    {
        await ValidateExecute(this.GetContextFor_ExecuteCode_Error(), true);
    }

    private static async Task ValidateExecute(ExecutionContext context, bool shouldFail)
    {
        // Arrange
        var codeExecutionService = GetCodeExecutionService();

        // Act
        var executionResult = await codeExecutionService.ExecuteAsync(context);

        // Arrange
        if (shouldFail)
        {
            executionResult.ErrorMessage.Should().NotBeNullOrEmpty();
        }
        else
        {
            executionResult.Output.Should().Be("42");
        }
    }

    protected abstract ExecutionContext GetContextFor_ExecuteCodeWithInput_Success();
    protected abstract ExecutionContext GetContextFor_ExecuteCodeWithoutInput_Success();
    protected abstract ExecutionContext GetContextFor_ExecuteCode_Error();

    private static ICodeExecutionService GetCodeExecutionService()
    {
        return new CodeExecutionService(
            new ProjectCreationService(new NullLogger<ProjectCreationService>()),
            new DockerExecutor(
                new DefaultDockerBuildArgsFactory(),
                new CodeExecutionSettings
                {
                    Timeout = TimeSpan.FromMinutes(1),
                    Docker = new DockerSettings
                    {
                        CpuLimit = 0.5,
                        MemoryMbLimit = 256,
                        PidLimit = 10
                    },
                    CSharp = new CSharpSettings
                    {
                        DotnetSdkImageName = "mcr.microsoft.com/dotnet/sdk:8.0",
                        DotnetRuntimeImageName = "mcr.microsoft.com/dotnet/runtime:8.0",
                        FrameworkVersion = "net8.0",
                        LanguageVersion = "12.0"
                    },
                    Python = new PythonSettings
                    {
                        ImageName = "python:3.11-slim"
                    }
                },
                new NullLogger<DockerExecutor>()));
    }
}