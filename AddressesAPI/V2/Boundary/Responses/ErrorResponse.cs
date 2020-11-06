using System.Collections.Generic;
using System.Linq;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using FluentValidation.Results;
using Newtonsoft.Json;

namespace AddressesAPI.V2.Boundary.Responses
{
    public class ErrorResponse
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("errors")]
        public IEnumerable<ValidationError> Errors { get; set; }

        public ErrorResponse() { }

        public ErrorResponse(ValidationResult validationResult)
        {
            var errors = validationResult.Errors
                .Select(validationResultError => new ValidationError(validationResultError)).ToList();

            Errors = errors;
        }

        public ErrorResponse(IList<ValidationError> validationResult)
        {
            Errors = validationResult;
        }
    }
}
