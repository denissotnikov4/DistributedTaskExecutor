using DistributedTaskExecutor.Core.RabbitMQ;
using Microsoft.Extensions.Logging;
using TaskService.Client;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;
using WorkerService.Cli.Models;
using WorkerService.Cli.Services.CodeExecution;
using ExecutionContext = WorkerService.Cli.Models.ExecutionContext;
using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;

namespace WorkerService.Cli.Services;

public class WorkerService : IWorkerService
{
    private readonly ICodeExecutionService codeExecutionService;
    private readonly IRabbitMessageQueue<Guid> taskIdMessageQueue;
    private readonly ITaskServiceClient taskServiceClient;
    private readonly ILogger<WorkerService> logger;

    public WorkerService(
        ICodeExecutionService codeExecutionService,
        IRabbitMessageQueue<Guid> taskIdMessageQueue,
        ITaskServiceClient taskServiceClient,
        ILogger<WorkerService> logger)
    {
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.taskIdMessageQueue = taskIdMessageQueue ?? throw new ArgumentNullException(nameof(taskIdMessageQueue));
        this.taskServiceClient = taskServiceClient ?? throw new ArgumentNullException(nameof(taskServiceClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync()
    {
        this.taskIdMessageQueue.Consume(this.ProcessTask);

        await Task.Delay(Timeout.Infinite);
    }

    private async Task<bool> ProcessTask(Guid taskId)
    {
        this.logger.LogInformation("Received Task \"{taskId}\", processing started.", taskId);

        var getTaskResult = await this.taskServiceClient.GetTaskByIdAsync(taskId);

        if (!getTaskResult.IsSuccess)
        {
            // TODO: Добавить отдельное поле для вида ошибки в Result.
            return getTaskResult.Error.Message == "Resource not found";
        }

        var (isInternalError, result) = await this.GetExecutionResult(GetExecutionContext(getTaskResult.Value));

        var updateResult = await this.taskServiceClient.UpdateTaskAsync(
            taskId,
            new TaskUpdateRequest
            {
                Status = isInternalError ? TaskStatus.Failed : TaskStatus.Completed,
                Result = result.Output,
                ErrorMessage = result.ErrorMessage
            });

        if (!updateResult.IsSuccess)
        {
            return false;
        }

        this.logger.LogInformation("Task \"{taskId}\" successfully processed.", taskId);

        return true;
    }

    private async Task<(bool isInternalError, ExecutionResult result)> GetExecutionResult(ExecutionContext context)
    {
        try
        {
            var result = await this.codeExecutionService.ExecuteAsync(context);

            return (false, new ExecutionResult { Output = result.Output + result.ErrorMessage });
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "An error occured while code execution process.");

            return (true, new ExecutionResult { ErrorMessage = exception.Message });
        }
    }

    private static ExecutionContext GetExecutionContext(ClientTask task)
    {
        return new ExecutionContext
        {
            Name = task.Id.ToString(),
            Code = task.Code,
            Language = task.Language,
            Input = task.InputData
        };
    }
}