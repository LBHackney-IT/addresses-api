using System;
using System.Net;

namespace AddressesAPI.V1.Boundary.Responses
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; protected set; }

        public ApiException()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        public ApiException(Exception ex) : base(ex?.Message, ex)
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        public ApiException(HttpStatusCode status)
        {
            StatusCode = status;
        }

        public ApiException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
