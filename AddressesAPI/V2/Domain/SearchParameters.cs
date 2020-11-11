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
        public int? Usrn { get; set; }
        public string UsagePrimary { get; set; }
        public string UsageCode { get; set; }
        public GlobalConstants.Format Format { get; set; }
        public IEnumerable<string> AddressStatus { get; set; }
        public bool OutOfBoroughAddress { get; set; }
        public bool IncludeParentShells { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
