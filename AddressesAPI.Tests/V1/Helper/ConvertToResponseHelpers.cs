using System.Net.Http;
using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using Newtonsoft.Json;

namespace AddressesAPI.Tests.V1.Helper
{
    public static class ConvertToResponseHelpers
    {
        public static async Task<APIResponse<SearchAddressResponse>> ConvertToSearchAddressResponseObject(this HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            return JsonConvert.DeserializeObject<APIResponse<SearchAddressResponse>>(data);
        }

        public static async Task<ErrorResponse> ConvertToErrorResponseObject(this HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            return JsonConvert.DeserializeObject<ErrorResponse>(data);
        }
    }
}
