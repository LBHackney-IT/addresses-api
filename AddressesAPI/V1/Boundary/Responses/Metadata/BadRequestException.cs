using System.Net;

namespace AddressesAPI.V1.Boundary.Responses.Metadata
{
    public class BadRequestException : ApiException
    {
        public RequestValidationResponse ValidationResponse { get; set; }

        public BadRequestException() : base(HttpStatusCode.BadRequest, "Request is null")
        {

        }

        public BadRequestException(RequestValidationResponse validationResponse)
        {
            StatusCode = HttpStatusCode.BadRequest;
            ValidationResponse = validationResponse;
        }
    }
}
