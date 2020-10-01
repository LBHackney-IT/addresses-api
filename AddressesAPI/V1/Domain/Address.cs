namespace AddressesAPI.V1.Domain
{
    public class Address : SimpleAddress
    {
        public new string Line1 { get; set; }
        public new string Line2 { get; set; }
        public new string Line3 { get; set; }
        public new string Line4 { get; set; }
        public new string Town { get; set; }
        public new string Postcode { get; set; }
        public new long UPRN { get; set; }

        //address detailed fields
        public string AddressKey { get; set; }
        public int? USRN { get; set; }
        public long? ParentUPRN { get; set; } //nullable
        public string AddressStatus { get; set; } //1 = "Approved Preferred", 3 = "Alternative", 5 = "Candidate", 6 = "Provisional", 7 = "Rejected External",  8 = "Historical", 9 = "Rejected Internal"
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
        public bool PropertyShell { get; set; }
        public bool? HackneyGazetteerOutOfBoroughAddress { get; set; } //for LLPG results; should be null in results for NLPG
        public double Easting { get; set; }
        public double Northing { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int addressStartDate { get; set; }
        public int addressEndDate { get; set; }
        public int addressChangeDate { get; set; }
        public int propertyStartDate { get; set; }
        public int propertyEndDate { get; set; }
        public int propertyChangeDate { get; set; }
    }
}
