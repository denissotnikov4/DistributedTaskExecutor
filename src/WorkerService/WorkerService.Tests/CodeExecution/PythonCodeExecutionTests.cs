using TaskService.Client.Models.Tasks;
using ExecutionContext = WorkerService.Cli.Models.ExecutionContext;

namespace WorkerService.Tests.CodeExecution;

[TestFixture]
public class PythonCodeExecutionTests : CodeExecutionTestsBase
{
    protected override ExecutionContext GetContextFor_ExecuteCodeWithInput_Success()
    {
        return new ExecutionContext
        {
            Name = Guid.NewGuid().ToString(),
            Code = "import sys\ndata=sys.stdin.read()\nprint(len(data), end=\"\")",
            Input = new string('a', 42),
            Language = ProgrammingLanguage.Python
        };
    }

    protected override ExecutionContext GetContextFor_ExecuteCodeWithoutInput_Success()
    {
        return new ExecutionContext
        {
            Name = Guid.NewGuid().ToString(),
            Code = "print(42, end=\"\")",
            Language = ProgrammingLanguage.Python
        };
    }

    protected override ExecutionContext GetContextFor_ExecuteCode_Error()
    {
        return new ExecutionContext
        {
            Name = Guid.NewGuid().ToString(),
            Code = "print(42 / 0, end=\"\")",
            Language = ProgrammingLanguage.Python
        };
    }
}