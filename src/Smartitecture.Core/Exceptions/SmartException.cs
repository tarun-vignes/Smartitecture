using System.Net;

namespace Smartitecture.Core.Exceptions
{
    public class SmartException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorCode { get; }
        public string[]? Details { get; }

        public SmartException(string message, 
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError, 
            string errorCode = "GENERIC_ERROR", 
            string[]? details = null) 
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Details = details;
        }
    }

    public class ValidationException : SmartException
    {
        public ValidationException(string message, string[] details) 
            : base(message, HttpStatusCode.BadRequest, "VALIDATION_ERROR", details)
        {
        }
    }

    public class NotFoundException : SmartException
    {
        public NotFoundException(string message) 
            : base(message, HttpStatusCode.NotFound, "NOT_FOUND")
        {
        }
    }

    public class UnauthorizedException : SmartException
    {
        public UnauthorizedException(string message) 
            : base(message, HttpStatusCode.Unauthorized, "UNAUTHORIZED")
        {
        }
    }

    public class ForbiddenException : SmartException
    {
        public ForbiddenException(string message) 
            : base(message, HttpStatusCode.Forbidden, "FORBIDDEN")
        {
        }
    }
}
