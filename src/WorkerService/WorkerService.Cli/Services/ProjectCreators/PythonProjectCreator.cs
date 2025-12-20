using WorkerService.Cli.Services.ProjectCreators.Base;
using WorkerService.Cli.Services.ProjectCreators.Models;

namespace WorkerService.Cli.Services.ProjectCreators;

public class PythonProjectCreator : ProjectCreatorBase
{
    public PythonProjectCreator()
        : base(Constants.Python.LanguageName)
    {
    }
}