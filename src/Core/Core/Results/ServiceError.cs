using System.Net;

namespace Core.Results;

public class ServiceError : Error
{
    public HttpStatusCode HttpStatus { get; }

    protected ServiceError(string message, HttpStatusCode httpStatus) : base(message)
    {
        HttpStatus = httpStatus;
    }

    public static ServiceError NotFound(string message = "") => new(message, HttpStatusCode.NotFound);

    public static ServiceError BadRequest(string message = "") => new(message, HttpStatusCode.BadRequest);

    public static ServiceError Forbidden(string message = "") => new(message, HttpStatusCode.Forbidden);

    public static ServiceError Conflict(string message = "") => new(message, HttpStatusCode.Conflict);
}