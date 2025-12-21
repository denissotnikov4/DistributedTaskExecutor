using TaskService.Client.Models.Tasks;
using WorkerService.Cli.Services.ProjectCreators.Base;

namespace WorkerService.Cli.Services.ProjectCreators;

public class PythonProjectCreator : ProjectCreatorBase
{
    public PythonProjectCreator()
        : base(ProgrammingLanguage.Python)
    {
    }
}