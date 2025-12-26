using ApiKeys.Client.Auth;

namespace ApiKeys.Client;

public static class HttpClientExtensions
{
    /// <summary>
    /// Добавляет ApiKey в заголовки запросов HttpClient
    /// </summary>
    public static HttpClient AddApiKey(this HttpClient httpClient, string apiKey, string? headerName = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
        }

        var header = headerName ?? ApiKeyAuthConstants.HeaderName;
        httpClient.DefaultRequestHeaders.Add(header, apiKey);

        return httpClient;
    }
}

