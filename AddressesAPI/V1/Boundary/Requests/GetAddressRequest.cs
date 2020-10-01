using AddressesAPI.V1.Boundary.Requests.RequestValidators;
using AddressesAPI.V1.Boundary.Responses;

namespace AddressesAPI.V1.Boundary.Requests
{
    public class GetAddressRequest
    {
        /// <summary>
        /// Exact match
        /// </summary>
        public string addressID { get; set; }
        /// <summary>
        /// Responsible for validating itself.
        /// Uses SearchAddressRequestValidator to do complex validation
        /// Sets defaults for Page and PageSize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns>RequestValidationResponse</returns>
        public RequestValidationResponse Validate<T>(T request)
        {
            if (request == null)
                return new RequestValidationResponse(false, "request is null");
            var validator = new GetAddressRequestValidator();
            var castedRequest = request as GetAddressRequest;
            if (castedRequest == null)
                return new RequestValidationResponse(false);
            var validationResult = validator.Validate(castedRequest);

            return new RequestValidationResponse(validationResult);
        }
    }
}
