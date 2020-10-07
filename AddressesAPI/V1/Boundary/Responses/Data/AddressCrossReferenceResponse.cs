using System;
using System.ComponentModel.DataAnnotations;

namespace AddressesAPI.V1.Boundary.Responses.Data
{
    public class AddressCrossReferenceResponse
    {
        public string CrossRefKey { get; set; }
        public long UPRN { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
