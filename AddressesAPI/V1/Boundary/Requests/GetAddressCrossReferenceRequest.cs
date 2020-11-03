using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;

namespace AddressesAPI.V1.Boundary.Requests
{
    public class GetAddressCrossReferenceRequest
    {

        /// <summary>
        /// Exact match
        /// </summary>
        public long uprn { get; set; }

        /// <summary>
        /// Responsible for validating itself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns>RequestValidationResponse</returns>
        public RequestValidationResponse Validate<T>(T request)
        {
            if (request == null)
                return new RequestValidationResponse(false, "request is null");
            var validator = new GetAddressCrossReferenceRequestValidator();
            var castedRequest = request as GetAddressCrossReferenceRequest;
            if (castedRequest == null)
                return new RequestValidationResponse(false);
            var validationResult = validator.Validate(castedRequest);

            return new RequestValidationResponse(validationResult);
        }
    }
}
