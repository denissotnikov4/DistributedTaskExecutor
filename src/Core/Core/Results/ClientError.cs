using System.Net;

namespace Core.Results;

/// <summary>
/// Ошибка, возникающая при работе с HTTP-клиентами
/// </summary>
public class ClientError : Error
{
    public HttpStatusCode StatusCode { get; }

    public Uri? RequestUri { get; }

    public string? Details { get; }

    private ClientError(
        string message,
        HttpStatusCode statusCode,
        Uri? requestUri = null,
        string? details = null)
        : base(message)
    {
        this.StatusCode = statusCode;
        this.RequestUri = requestUri;
        this.Details = details;
    }

    public static ClientError FromStatusCode(
        HttpStatusCode statusCode,
        string? message = null,
        Uri? requestUri = null,
        string? details = null)
    {
        var defaultMessage = statusCode switch
        {
            HttpStatusCode.NotFound => "Resource not found",
            HttpStatusCode.BadRequest => "Bad request",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.InternalServerError => "Internal server error",
            HttpStatusCode.ServiceUnavailable => "Service unavailable",
            HttpStatusCode.RequestTimeout => "Request timeout",
            _ => $"HTTP error: {statusCode}"
        };

        return new ClientError(
            message ?? defaultMessage,
            statusCode,
            requestUri,
            details);
    }

    public static ClientError NetworkError(string message, Uri? requestUri = null, Exception? innerException = null)
    {
        return new ClientError(
            message,
            HttpStatusCode.ServiceUnavailable,
            requestUri,
            innerException?.Message);
    }

    public static ClientError DeserializationError(string message, Uri? requestUri = null)
    {
        return new ClientError(
            message,
            HttpStatusCode.InternalServerError,
            requestUri);
    }
}