using WorkerService.Cli.Settings.CodeExecution;

namespace WorkerService.Cli.Services.CodeExecution.Executors.Docker;

public class DefaultDockerBuildArgsFactory : IDockerBuildArgsFactory
{
    public Dictionary<string, string> GetCSharpBuildArgs(CSharpSettings settings)
    {
        return new Dictionary<string, string>
        {
            ["SDK_IMAGE"] = settings.DotnetSdkImageName,
            ["RUNTIME_IMAGE"] = settings.DotnetRuntimeImageName,
            ["FRAMEWORK_VERSION"] = settings.FrameworkVersion,
            ["LANGUAGE_VERSION"] = settings.LanguageVersion
        };
    }

    public Dictionary<string, string> GetPythonBuildArgs(PythonSettings settings)
    {
        return new Dictionary<string, string>
        {
            ["LANGUAGE_IMAGE"] = settings.ImageName
        };
    }
}