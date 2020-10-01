using System.Collections.Generic;
using AddressesAPI.V1.Boundary.Responses.Data;
using Newtonsoft.Json;

namespace AddressesAPI.V1.Boundary.Responses
{
    public class GetAddressCrossReferenceResponse
    {
        [JsonProperty("addresscrossreferences")]
        public List<AddressCrossReference> AddressCrossReferences { get; set; }

    }
}
