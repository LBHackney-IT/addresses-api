using System.Collections.Generic;
namespace AddressesAPI.V1.Domain
{
    public class Address
    {
        //address detailed fields
        public string AddressKey { get; set; }
        public int? USRN { get; set; }
        public long? ParentUPRN { get; set; } //nullable
        public string AddressStatus { get; set; } //1 = "Approved", 3 = "Alternative", 5 = "Candidate", 6 = "Provisional", 7 = "Rejected External",  8 = "Historical", 9 = "Rejected Internal"
        public string UnitName { get; set; }
        public string UnitNumber { get; set; } //string because can be e.g. "1a"
        public string BuildingName { get; set; }
        public string BuildingNumber { get; set; }
        public string Street { get; set; }
        public string Locality { get; set; } //for NLPG results; should be null in results for LLPG
        public string Gazetteer { get; set; } //“hackney” or “national”
        public string CommercialOccupier { get; set; }
        public string Ward { get; set; }
        public string UsageDescription { get; set; }
        public string UsagePrimary { get; set; }
        public string UsageCode { get; set; }
        public string PlanningUseClass { get; set; }
        public bool? PropertyShell { get; set; }
        public bool? HackneyGazetteerOutOfBoroughAddress { get; set; } //for LLPG results; should be null in results for NLPG
        public double? Easting { get; set; }
        public double? Northing { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public int? AddressStartDate { get; set; }
        public int? AddressEndDate { get; set; }
        public int? AddressChangeDate { get; set; }
        public int? PropertyStartDate { get; set; }
        public int? PropertyEndDate { get; set; }
        public int? PropertyChangeDate { get; set; }

        // address simple fields
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
        public long UPRN { get; set; }
        //to hold child addresses in a hierarchy
        public List<Address> ChildAddresses { get; set; }
    }
}
