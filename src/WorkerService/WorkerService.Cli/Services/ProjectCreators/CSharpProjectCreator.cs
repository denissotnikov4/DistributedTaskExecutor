using TaskService.Client.Models.Tasks;
using WorkerService.Cli.Exceptions;
using WorkerService.Cli.Helpers;
using WorkerService.Cli.Models;
using WorkerService.Cli.Services.ProjectCreators.Base;
using WorkerService.Cli.Settings.CodeExecution;

namespace WorkerService.Cli.Services.ProjectCreators;

public class CSharpProjectCreator : ProjectCreatorBase
{
    private readonly CSharpSettings settings;
    public CSharpProjectCreator(CSharpSettings settings)
        : base(ProgrammingLanguage.CSharp)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    protected override async Task InitProjectIfNeededAsync(string projectPath)
    {
        var dotnetNewResult = await CommandsHelper.DotnetNew(
            projectPath,
            Constants.DefaultProjectName,
            this.settings.FrameworkVersion,
            this.settings.LanguageVersion);

        dotnetNewResult.ThrowIfFailed(() => new DotnetProjectInitException(dotnetNewResult.Stderr!));
    }
}