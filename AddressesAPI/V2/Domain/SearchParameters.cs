using System;
using System.Collections.Generic;

namespace AddressesAPI.V2.Domain
{
    public class SearchParameters
    {
        public string Postcode { get; set; }
        public string BuildingNumber { get; set; }
        public string Street { get; set; }
        public GlobalConstants.Gazetteer Gazetteer { get; set; }
        public long? Uprn { get; set; }
        public List<long> CrossReferencedUprns { get; set; }
        public int? Usrn { get; set; }
        public string UsagePrimary { get; set; }
        public string UsageCode { get; set; }
        public IEnumerable<string> AddressStatus { get; set; }
        public bool OutOfBoroughAddress { get; set; }
        public bool IncludePropertyShells { get; set; }
        public string CrossRefCode { get; set; }
        public string CrossRefValue { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string AddressQuery { get; set; }
        public DateTime? ModifiedSince { get; set; }
    }
}
