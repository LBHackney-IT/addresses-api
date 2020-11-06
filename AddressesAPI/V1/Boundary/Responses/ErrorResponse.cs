using System;
using System.Collections.Generic;
using System.Linq;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using FluentValidation.Results;
using Newtonsoft.Json;

namespace AddressesAPI.V1.Boundary.Responses
{
    public class ErrorResponse
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("error")]
        public APIError Error { get; set; }

        public ErrorResponse() { }

        public ErrorResponse(ValidationResult validationResult)
        {
            var errors = validationResult.Errors
                .Select(validationResultError => new ValidationError(validationResultError)).ToList();

            Error = new APIError { IsValid = validationResult.IsValid, ValidationErrors = errors };
        }

        public ErrorResponse(IList<ValidationError> validationResult)
        {
            Error = new APIError { IsValid = !validationResult.Any(), ValidationErrors = validationResult };
        }
    }
}
