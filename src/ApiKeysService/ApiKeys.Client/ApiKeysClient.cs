using ApiKeys.Client.Models;
using DistributedTaskExecutor.Core.Results;

namespace ApiKeys.Client;

public interface IApiKeysClient
{
    Task<ClientResult<ApiKeyValidationResult>> ValidateApiKeyAsync(ValidateApiKeyRequest request);
}

public class ApiKeysClient(HttpClient httpClient) : IApiKeysClient
{
    private const string ApiKeysPath = "api/apikeys";

    public async Task<ClientResult<ApiKeyValidationResult>> ValidateApiKeyAsync(ValidateApiKeyRequest request)
    {
        return await httpClient.PostAsResultAsync<ValidateApiKeyRequest, ApiKeyValidationResult>(
            $"{ApiKeysPath}/validate",
            request);
    }
}