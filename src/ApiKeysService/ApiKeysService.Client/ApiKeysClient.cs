using System.Net;
using System.Net.Http.Json;
using ApiKeysService.Client.Models;

namespace ApiKeysService.Client;

public interface IApiKeysClient
{
    Task<ApiKeyCreateResponse> CreateApiKeyAsync(
        ApiKeyCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiKeyInfo?> GetApiKeyInfoAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ICollection<ApiKeyInfo>> GetAllApiKeysAsync(CancellationToken cancellationToken = default);
    Task UpdateApiKeyAsync(Guid id, ApiKeyUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteApiKeyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
}

public class ApiKeysClient(HttpClient httpClient) : IApiKeysClient
{
    private const string BasePath = "api/apikeys";

    public async Task<ApiKeyCreateResponse> CreateApiKeyAsync(ApiKeyCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(BasePath, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiKeyCreateResponse>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<ApiKeyInfo?> GetApiKeyInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"{BasePath}/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiKeyInfo>(cancellationToken: cancellationToken);
    }

    public async Task<ICollection<ApiKeyInfo>> GetAllApiKeysAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(BasePath, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result =
            await response.Content.ReadFromJsonAsync<ICollection<ApiKeyInfo>>(cancellationToken: cancellationToken);
        return result ?? new List<ApiKeyInfo>();
    }

    public async Task UpdateApiKeyAsync(Guid id, ApiKeyUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"{BasePath}/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteApiKeyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"{BasePath}/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey,
        CancellationToken cancellationToken = default)
    {
        var request = new { ApiKey = apiKey };
        var response = await httpClient.PostAsJsonAsync($"{BasePath}/validate", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiKeyValidationResult>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}