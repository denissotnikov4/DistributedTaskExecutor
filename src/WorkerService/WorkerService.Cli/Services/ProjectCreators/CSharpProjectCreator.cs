using TaskService.Client.Models.Tasks;
using WorkerService.Cli.Exceptions;
using WorkerService.Cli.Helpers;
using WorkerService.Cli.Services.ProjectCreators.Base;

namespace WorkerService.Cli.Services.ProjectCreators;

public class CSharpProjectCreator : ProjectCreatorBase
{
    public CSharpProjectCreator()
        : base(ProgrammingLanguage.CSharp)
    {
    }

    protected override async Task InitProjectIfNeededAsync(string projectPath)
    {
        var dotnetNewResult = await CommandsHelper.DotnetNew(projectPath);

        dotnetNewResult.ThrowIfFailed(() => new DotnetProjectInitException(dotnetNewResult.Stderr!));
    }
}