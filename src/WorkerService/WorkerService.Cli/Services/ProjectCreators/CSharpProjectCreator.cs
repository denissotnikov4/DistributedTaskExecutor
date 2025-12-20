using WorkerService.Cli.Helpers;
using WorkerService.Cli.Services.ProjectCreators.Base;
using WorkerService.Cli.Services.ProjectCreators.Models;

namespace WorkerService.Cli.Services.ProjectCreators;

public class CSharpProjectCreator : ProjectCreatorBase
{
    public CSharpProjectCreator()
        : base(Constants.CSharp.LanguageName)
    {
    }

    protected override async Task InitProjectIfNeededAsync(string projectPath)
    {
        var (_, error, _) = await ProcessHelper.RunProcessAsync(
            "dotnet",
            $"new console --framework net8.0 --langVersion 12.0 -o \"{projectPath}\" -n App"); // Вынести дефолтное имя проекта в const

        if (!string.IsNullOrWhiteSpace(error))
        {
            throw new Exception(error); // Спец. тип исключения.
        }
    }
}