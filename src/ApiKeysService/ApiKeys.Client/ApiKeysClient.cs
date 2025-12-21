using System.Net.Http.Json;
using ApiKeys.Client.Models;

namespace ApiKeys.Client;

public interface IApiKeysClient
{
    Task<ApiKeyValidationResult> ValidateApiKeyAsync(ValidateApiKeyRequest request);
}

public class ApiKeysClient(HttpClient httpClient) : IApiKeysClient
{
    private const string ApiKeysPath = "api/apikeys";

    public async Task<ApiKeyValidationResult> ValidateApiKeyAsync(ValidateApiKeyRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"{ApiKeysPath}/validate", request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"API key validation failed with status {response.StatusCode}: {errorMessage}");
        }
        
        return await response.Content.ReadFromJsonAsync<ApiKeyValidationResult>()
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}