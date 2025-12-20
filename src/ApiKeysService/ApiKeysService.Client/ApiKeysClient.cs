using System.Net;
using System.Net.Http.Json;
using ApiKeysService.Client.Models;

namespace ApiKeysService.Client;

public interface IApiKeysClient
{
    Task<ApiKeyInfo?> GetApiKeyInfoAsync(Guid id);
    Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey);
}

public class ApiKeysClient(HttpClient httpClient) : IApiKeysClient
{
    private const string ApiKeysPath = "api/apikeys";

    public async Task<ApiKeyInfo?> GetApiKeyInfoAsync(Guid id)
    {
        var response = await httpClient.GetAsync($"{ApiKeysPath}/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiKeyInfo>();
    }

    public async Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey)
    {
        var request = new { ApiKey = apiKey };
        var response = await httpClient.PostAsJsonAsync($"{ApiKeysPath}/validate", request);
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiKeyValidationResult>()
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}