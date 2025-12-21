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
        var response = await httpClient.PostAsJsonAsync($"{ApiKeysPath}/validate", apiKey);
        
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