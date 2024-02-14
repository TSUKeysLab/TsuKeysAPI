using System.Net;

namespace tsuKeysAPIProject.AdditionalServices.Exceptions
{
    public class Exceptions : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public Exceptions(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException(string message) : base(message)
        {
        }
    }
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
