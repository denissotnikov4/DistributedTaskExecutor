using System.Net.Http.Json;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;

namespace TaskService.Client.Api.Tasks;

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

    public async Task<RequestResult<ClientTask>> GetTaskByIdAsync(Guid id, CancellationToken cancelToken = default)
    {
        try
        {
            this.logInfo($"Trying to GET Task by id \"{id}\".");

            var response = await this.httpClient.GetAsync($"{ApiPath}/{id}", cancelToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync(cancelToken)
                    .ConfigureAwait(false);

                this.logError($"Can't get Task by id \"{id}\" due to error. StatusCode: {(int)response.StatusCode}, Message: {errorMessage}.");

                return RequestResult<ClientTask>.Failure((int)response.StatusCode, errorMessage);
            }

            this.logInfo($"Successfully GOT Task by id \"{id}\".");

            var result = await response.Content.ReadFromJsonAsync<ClientTask>(cancellationToken: cancelToken)
                .ConfigureAwait(false);

            return RequestResult<ClientTask>.Success(result!, (int)response.StatusCode);
        }
        catch (Exception exception)
        {
            var errorMessage = $"Unexpected error occurred while GETTING Task by id \"{id}\": {exception.Message}.";

            this.logError(errorMessage);

            return RequestResult<ClientTask>.Failure(500, errorMessage);
        }
    }

    public async Task<RequestResult> UpdateTaskAsync(
        Guid id,
        TaskUpdateRequest updateRequest,
        CancellationToken cancelToken = default)
    {
        try
        {
            this.logInfo($"Trying to PATCH Task by id \"{id}\".");

            var response = await this.httpClient.PatchAsJsonAsync($"{ApiPath}/{id}", updateRequest, cancelToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancelToken)
                    .ConfigureAwait(false);

                this.logError($"Can't PATCH Task by id \"{id}\" due to error. StatusCode: {(int)response.StatusCode}, Message: {content}.");

                return RequestResult.Failure((int)response.StatusCode, content);
            }

            this.logInfo($"Successfully PATCHED Task by id \"{id}\".");

            return RequestResult.Success((int)response.StatusCode);
        }
        catch (Exception exception)
        {
            var errorMessage = $"Unexpected error occurred while PATCHING Task by id \"{id}\": {exception.Message}.";

            this.logError(errorMessage);

            return RequestResult.Failure(500, errorMessage);
        }
    }
}