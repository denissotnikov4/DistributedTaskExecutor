using WorkerService.Cli.Settings.CodeExecution;

namespace WorkerService.Cli.Services.CodeExecution.Executors.Docker;

public interface IDockerBuildArgsFactory
{
    Dictionary<string, string> GetCSharpBuildArgs(CSharpSettings settings);

    Dictionary<string, string> GetPythonBuildArgs(PythonSettings settings);
}