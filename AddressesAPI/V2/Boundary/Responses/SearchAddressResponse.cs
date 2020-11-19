using System.Collections.Generic;
using AddressesAPI.V2.Boundary.Responses.Data;
using Newtonsoft.Json;

namespace AddressesAPI.V2.Boundary.Responses
{
    public class SearchAddressResponse
    {
        [JsonProperty("address")]
        public List<AddressResponse> Addresses { get; set; }

        [JsonProperty("pageCount")]
        public long PageCount { get; set; }
        [JsonProperty("totalCount")]
        public long TotalCount { get; set; }
    }
}
