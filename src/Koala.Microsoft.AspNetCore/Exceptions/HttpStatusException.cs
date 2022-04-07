using System;
using System.Net;

namespace Microsoft.AspNetCore.Exceptions
{
    public class HttpStatusException : Exception
    {
        public HttpStatusCode Status { get; private set; }

        public HttpStatusException(HttpStatusCode status, string message) : base(message)
        {
            Status = status;
        }

        public HttpStatusException(HttpStatusCode status, string message, Exception exception) : base($"{message} {exception.Message}", exception)
        {
            Status = status;
        }

        public HttpStatusException(HttpStatusCode status, Exception exception) : base(exception.Message, exception)
        {
            Status = status;
        }
    }
}