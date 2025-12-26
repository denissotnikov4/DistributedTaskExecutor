using System.Net.Http.Json;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;

namespace TaskService.Client.Api.Tasks;

public class TaskServiceClient : ITaskServiceClient
{
    private const string ApiPath = "api/tasks";

    private readonly HttpClient httpClient;
    private readonly Action<LogLevel, string> log;

    public TaskServiceClient(string baseUrl, string apiKey, Action<LogLevel, string> log)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentNullException.ThrowIfNull(log);

        this.httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        this.httpClient.DefaultRequestHeaders.Add("X-ApiKey", apiKey);

        this.log = log;
    }

    public async Task<RequestResult<ClientTask>> GetTaskByIdAsync(Guid id, CancellationToken cancelToken = default)
    {
        try
        {
            this.log(LogLevel.Info, $"Trying to GET Task by id \"{id}\".");

            var response = await this.httpClient.GetAsync($"{ApiPath}/{id}", cancelToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync(cancelToken);

                this.log(
                    LogLevel.Error,
                    $"Can't get Task by id \"{id}\" due to error. StatusCode: {(int)response.StatusCode}, Message: {errorMessage}.");

                return RequestResult<ClientTask>.Failure((int)response.StatusCode, errorMessage);
            }

            this.log(LogLevel.Info, $"Successfully GOT Task by id \"{id}\".");

            var result = await response.Content.ReadFromJsonAsync<ClientTask>(cancellationToken: cancelToken);

            return RequestResult<ClientTask>.Success(result!, (int)response.StatusCode);
        }
        catch (Exception exception)
        {
            var errorMessage = $"Unexpected error occurred while GETTING Task by id \"{id}\": {exception.Message}.";

            this.log(LogLevel.Error, errorMessage);

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
            this.log(LogLevel.Info, $"Trying to PATCH Task by id \"{id}\".");

            var response = await this.httpClient.PatchAsJsonAsync($"{ApiPath}/{id}", updateRequest, cancelToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancelToken);

                this.log(
                    LogLevel.Error,
                    $"Can't PATCH Task by id \"{id}\" due to error. StatusCode: {(int)response.StatusCode}, Message: {content}.");

                return RequestResult.Failure((int)response.StatusCode, content);
            }

            this.log(LogLevel.Info, $"Successfully PATCHED Task by id \"{id}\".");

            return RequestResult.Success((int)response.StatusCode);
        }
        catch (Exception exception)
        {
            var errorMessage = $"Unexpected error occurred while PATCHING Task by id \"{id}\": {exception.Message}.";

            this.log(LogLevel.Error, errorMessage);

            return RequestResult.Failure(500, errorMessage);
        }
    }
}