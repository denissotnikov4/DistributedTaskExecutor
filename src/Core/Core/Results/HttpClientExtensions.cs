using System.Net.Http.Json;

namespace Core.Results;

public static class HttpClientExtensions
{
    public static async Task<ClientResult<T>> GetAsResultAsync<T>(
        this HttpClient client,
        string? requestUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetAsync(requestUri, cancellationToken);
            return await HandleResponseAsync<T>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return ClientError.NetworkError(
                $"Network error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ClientError.NetworkError(
                "Request timeout",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (Exception ex)
        {
            return ClientError.NetworkError(
                $"Unexpected error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
    }

    public static async Task<ClientResult<TResponse>> PostAsResultAsync<TRequest, TResponse>(
        this HttpClient client,
        string? requestUri,
        TRequest value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.PostAsJsonAsync(requestUri, value, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return ClientError.NetworkError(
                $"Network error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ClientError.NetworkError(
                "Request timeout",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (Exception ex)
        {
            return ClientError.NetworkError(
                $"Unexpected error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
    }

    public static async Task<ClientResult> PostAsResultAsync<TRequest>(
        this HttpClient client,
        string? requestUri,
        TRequest value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.PostAsJsonAsync(requestUri, value, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return ClientError.NetworkError(
                $"Network error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ClientError.NetworkError(
                "Request timeout",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (Exception ex)
        {
            return ClientError.NetworkError(
                $"Unexpected error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
    }

    public static async Task<ClientResult<TResponse>> PutAsResultAsync<TRequest, TResponse>(
        this HttpClient client,
        string? requestUri,
        TRequest value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.PutAsJsonAsync(requestUri, value, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return ClientError.NetworkError(
                $"Network error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ClientError.NetworkError(
                "Request timeout",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (Exception ex)
        {
            return ClientError.NetworkError(
                $"Unexpected error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
    }

    public static async Task<ClientResult> PutAsResultAsync<TRequest>(
        this HttpClient client,
        string? requestUri,
        TRequest value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.PutAsJsonAsync(requestUri, value, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return ClientError.NetworkError(
                $"Network error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ClientError.NetworkError(
                "Request timeout",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (Exception ex)
        {
            return ClientError.NetworkError(
                $"Unexpected error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
    }

    public static async Task<ClientResult> DeleteAsResultAsync(
        this HttpClient client,
        string? requestUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.DeleteAsync(requestUri, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return ClientError.NetworkError(
                $"Network error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ClientError.NetworkError(
                "Request timeout",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
        catch (Exception ex)
        {
            return ClientError.NetworkError(
                $"Unexpected error: {ex.Message}",
                requestUri != null ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null,
                ex);
        }
    }

    private static async Task<ClientResult<T>> HandleResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var content = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
                if (content == null)
                {
                    return ClientError.DeserializationError(
                        "Failed to deserialize response",
                        response.RequestMessage?.RequestUri);
                }

                return content;
            }
            catch (Exception ex)
            {
                return ClientError.DeserializationError(
                    $"Deserialization error: {ex.Message}",
                    response.RequestMessage?.RequestUri);
            }
        }

        var errorMessage = await TryReadErrorMessage(response);
        return ClientError.FromStatusCode(
            response.StatusCode,
            errorMessage,
            response.RequestMessage?.RequestUri);
    }

    private static async Task<ClientResult> HandleResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return ClientResult.Success;
        }

        var errorMessage = await TryReadErrorMessage(response);
        return ClientError.FromStatusCode(
            response.StatusCode,
            errorMessage,
            response.RequestMessage?.RequestUri);
    }

    private static async Task<string?> TryReadErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(content) ? null : content;
        }
        catch
        {
            return null;
        }
    }
}