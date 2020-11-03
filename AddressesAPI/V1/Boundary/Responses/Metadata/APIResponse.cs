using System;
using System.Net;
using AddressesAPI.V1.Infrastructure;
using Newtonsoft.Json;

namespace AddressesAPI.V1.Boundary.Responses.Metadata
{
    /// <summary>
    /// API Response wrapper for all API responses
    /// If a request has been successful this will be denoted by the statusCode
    ///     Then the 'data' property will be populated
    /// If a request has not been successful denoted
    ///     Then the Error property will be populated
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class APIResponse<T> where T : class
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        public APIResponse() { }

        public APIResponse(T result)
        {
            StatusCode = (int) HttpStatusCode.OK;
            Data = result;
        }
    }

    public class ErrorResponse
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("error")]
        public APIError Error { get; set; }
        public ErrorResponse () {}
        public ErrorResponse(RequestValidationResponse ex)
        {
            StatusCode = (int) HttpStatusCode.BadRequest;
            Error = new APIError
            {
                IsValid = ex?.IsValid ?? false,
                ValidationErrors = ex?.ValidationErrors
            };
        }
    }
}
