using System.Collections.Generic;
using AddressesAPI.V2.Boundary.Responses.Data;
using Newtonsoft.Json;

namespace AddressesAPI.V2.Boundary.Responses
{
    public class SearchAddressResponse
    {
        [JsonProperty("address")]
        public List<AddressResponse> Addresses { get; set; }

        [JsonProperty("page_count")]
        public int PageCount { get; set; }
        [JsonProperty("total_count")]
        public int TotalCount { get; set; }
    }
}
