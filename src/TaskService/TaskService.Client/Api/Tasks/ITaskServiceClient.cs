using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;

namespace TaskService.Client.Api.Tasks;

public interface ITaskServiceClient
{
    Task<RequestResult<ClientTask>> GetTaskByIdAsync(Guid id, CancellationToken cancelToken = default);

    Task<RequestResult> UpdateTaskAsync(
        Guid id,
        TaskUpdateRequest updateRequest,
        CancellationToken cancelToken = default);
}