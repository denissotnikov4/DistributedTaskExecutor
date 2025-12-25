using TaskService.Client.Models.Tasks;
using ExecutionContext = WorkerService.Cli.Models.ExecutionContext;

namespace WorkerService.Tests.CodeExecution;

[TestFixture]
public class CSharpCodeExecutionTests : CodeExecutionTestsBase
{
    protected override ExecutionContext GetContextFor_ExecuteCodeWithInput_Success()
    {
        return new ExecutionContext
        {
            Name = Guid.NewGuid().ToString(),
            Code = "public static class Program { public static void Main() { var input = int.Parse(Console.In.ReadToEnd()); Console.Write(input + 1); } }",
            Input = "41",
            Language = ProgrammingLanguage.CSharp
        };
    }

    protected override ExecutionContext GetContextFor_ExecuteCodeWithoutInput_Success()
    {
        return new ExecutionContext
        {
            Name = Guid.NewGuid().ToString(),
            Code = "public static class Program { public static void Main() { Console.Write(42); } }",
            Language = ProgrammingLanguage.CSharp
        };
    }

    protected override ExecutionContext GetContextFor_ExecuteCode_Error()
    {
        return new ExecutionContext
        {
            Name = Guid.NewGuid().ToString(),
            Code = "public static class Program { public static void Main() { throw new Exception(); } }",
            Language = ProgrammingLanguage.CSharp
        };
    }
}