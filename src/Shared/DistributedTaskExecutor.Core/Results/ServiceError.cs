using System.Net;

namespace DistributedTaskExecutor.Core.Results;

public class ServiceError : Error
{

    protected ServiceError(string message, HttpStatusCode httpStatus) : base(message)
    {
        this.HttpStatus = httpStatus;
    }
    public HttpStatusCode HttpStatus { get; }

    public static ServiceError NotFound(string message = "")
    {
        return new ServiceError(message, HttpStatusCode.NotFound);
    }

    public static ServiceError BadRequest(string message = "")
    {
        return new ServiceError(message, HttpStatusCode.BadRequest);
    }

    public static ServiceError Forbidden(string message = "")
    {
        return new ServiceError(message, HttpStatusCode.Forbidden);
    }

    public static ServiceError Conflict(string message = "")
    {
        return new ServiceError(message, HttpStatusCode.Conflict);
    }

    public static ServiceError Unauthorized(string message = "")
    {
        return new ServiceError(message, HttpStatusCode.Unauthorized);
    }
}