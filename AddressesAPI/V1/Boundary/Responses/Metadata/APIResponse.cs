using System;
using System.Net;
using LBHAddressesAPI.Infrastructure.V1.API;
using Newtonsoft.Json;

namespace AddressesAPI.V1.Boundary.Responses
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

        [JsonProperty("error")]
        public APIError Error { get; set; }

        public APIResponse(BadRequestException ex)
        {
            StatusCode = (int) ex.StatusCode;
            Error = new APIError(ex?.ValidationResponse);
        }

        public APIResponse(ApiException ex)
        {
            StatusCode = (int) ex.StatusCode;
            Error = new APIError(ex);
        }

        public APIResponse(Exception ex)
        {
            StatusCode = (int) HttpStatusCode.InternalServerError;
            Error = new APIError(ex);
        }

        public APIResponse(T result)
        {
            StatusCode = (int) HttpStatusCode.OK;
            Data = result;
        }
    }
}
