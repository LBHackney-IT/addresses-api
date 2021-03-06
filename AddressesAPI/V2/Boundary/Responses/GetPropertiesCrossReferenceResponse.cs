using System.Collections.Generic;
using AddressesAPI.V2.Boundary.Responses.Data;
using Newtonsoft.Json;

namespace AddressesAPI.V2.Boundary.Responses
{
    public class GetPropertiesCrossReferenceResponse
    {
        [JsonProperty("addresscrossreferences")]
        public List<AddressCrossReferenceResponse> AddressCrossReferences { get; set; }

    }
}
