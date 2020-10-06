using System;
using System.ComponentModel.DataAnnotations;

namespace AddressesAPI.V1.Boundary.Responses.Data
{
    public class AddressCrossReferenceResponse
    {
        public string crossRefKey { get; set; }
        public long UPRN { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public DateTime? endDate { get; set; }
    }

}
