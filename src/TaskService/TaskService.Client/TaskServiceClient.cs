using DistributedTaskExecutor.Core.Results;
using Microsoft.Extensions.Logging;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;

namespace TaskService.Client;

public class TaskServiceClient : ITaskServiceClient
{
    private const string ApiPath = "api/tasks";

    private readonly HttpClient httpClient;
    private readonly ILogger<TaskServiceClient> logger;

    public TaskServiceClient(
        string baseUrl,
        string apiKey,
        ILogger<TaskServiceClient> logger)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentNullException(nameof(baseUrl));
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentNullException(nameof(apiKey));
        }

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        this.httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        this.httpClient.DefaultRequestHeaders.Add("X-ApiKey", apiKey);
    }

    public async Task<ClientResult<ClientTask>> GetTaskByIdAsync(Guid id, CancellationToken cancelToken = default)
    {
        this.logger.LogInformation("Trying to GET Task by id {TaskId}", id);

        var result = await this.httpClient.GetAsResultAsync<ClientTask>($"{ApiPath}/{id}", cancelToken);

        if (result.IsSuccess)
        {
            this.logger.LogInformation("Successfully GOT Task by id {TaskId}", id);
        }
        else
        {
            this.logger.LogError(
                "Can't get Task by id {TaskId} due to error. Error: {ErrorMessage}",
                id,
                result.Error.Message);
        }

        return result;
    }

    public async Task<ClientResult> UpdateTaskAsync(
        Guid id,
        TaskUpdateRequest updateRequest,
        CancellationToken cancelToken = default)
    {
        this.logger.LogInformation("Trying to PATCH Task by id {TaskId}", id);

        var result = await this.httpClient.PatchAsResultAsync($"{ApiPath}/{id}", updateRequest, cancelToken);

        if (result.IsSuccess)
        {
            this.logger.LogInformation("Successfully PATCHED Task by id {TaskId}", id);
        }
        else
        {
            this.logger.LogError(
                "Can't PATCH Task by id {TaskId} due to error. Error: {ErrorMessage}",
                id,
                result.Error.Message);
        }

        return result;
    }
}