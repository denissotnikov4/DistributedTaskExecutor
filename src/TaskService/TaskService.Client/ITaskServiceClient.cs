using Core.Results;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;

namespace TaskService.Client;

public interface ITaskServiceClient
{
    Task<ClientResult<ClientTask>> GetTaskByIdAsync(Guid id, CancellationToken cancelToken = default);

    Task<ClientResult> UpdateTaskAsync(
        Guid id,
        TaskUpdateRequest updateRequest,
        CancellationToken cancelToken = default);
}