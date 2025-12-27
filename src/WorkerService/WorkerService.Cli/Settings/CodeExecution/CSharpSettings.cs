using Vostok.Configuration.Abstractions.Attributes;

namespace WorkerService.Cli.Settings.CodeExecution;

public class CSharpSettings
{
    [Required]
    public string DotnetSdkImageName { get; init; }

    [Required]
    public string DotnetRuntimeImageName { get; init; }

    [Required]
    public string FrameworkVersion { get; init; }

    [Required]
    public string LanguageVersion { get; init; }
}