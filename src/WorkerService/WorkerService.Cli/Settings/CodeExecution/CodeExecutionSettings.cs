using Vostok.Configuration.Abstractions.Attributes;

namespace WorkerService.Cli.Settings.CodeExecution;

public class CodeExecutionSettings
{
    [Required]
    public TimeSpan? Timeout { get; init; }

    [Required]
    public DockerSettings Docker { get; init; }

    [Required]
    public CSharpSettings CSharp { get; init; }

    [Required]
    public PythonSettings Python { get; init; }
}