using Core.Results;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;

namespace TaskService.Client;

public class TaskServiceClient : ITaskServiceClient
{
    private const string ApiPath = "api/tasks";

    private readonly HttpClient httpClient;

    private readonly Action<string> logInfo;
    private readonly Action<string> logError;

    public TaskServiceClient(
        string baseUrl,
        string apiKey,
        Action<string> logInfo,
        Action<string> logError)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentNullException(nameof(baseUrl));
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentNullException(nameof(apiKey));
        }

        this.httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        this.httpClient.DefaultRequestHeaders.Add("X-ApiKey", apiKey);

        this.logInfo = logInfo ?? throw new ArgumentNullException(nameof(logInfo));
        this.logError = logError ?? throw new ArgumentNullException(nameof(logError));
    }

    public async Task<ClientResult<ClientTask>> GetTaskByIdAsync(Guid id, CancellationToken cancelToken = default)
    {
        this.logInfo($"Trying to GET Task by id \"{id}\".");

        var result = await this.httpClient.GetAsResultAsync<ClientTask>($"{ApiPath}/{id}", cancelToken)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            this.logInfo($"Successfully GOT Task by id \"{id}\".");
        }
        else
        {
            this.logError($"Can't get Task by id \"{id}\" due to error. Error: {result.Error.Message}.");
        }

        return result;
    }

    public async Task<ClientResult> UpdateTaskAsync(
        Guid id,
        TaskUpdateRequest updateRequest,
        CancellationToken cancelToken = default)
    {
        this.logInfo($"Trying to PATCH Task by id \"{id}\".");

        var result = await this.httpClient.PatchAsResultAsync($"{ApiPath}/{id}", updateRequest, cancelToken)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            this.logInfo($"Successfully PATCHED Task by id \"{id}\".");
        }
        else
        {
            this.logError($"Can't PATCH Task by id \"{id}\" due to error. Error: {result.Error.Message}.");
        }

        return result;
    }
}