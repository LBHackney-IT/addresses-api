using Newtonsoft.Json;

namespace AddressesAPI.V2.Boundary.Responses.Data
{
    public class AddressResponse
    {
        //Address simple responses
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }

        /// <summary>
        /// Requirement to return the address on a single line, comma separated, to save work in client applications.
        /// </summary>
        public string SingleLineAddress
        {
            get
            {
                // Build comma separated address
                var ret =
                    GetAddressComponent(Line1) +
                    GetAddressComponent(Line2) +
                    GetAddressComponent(Line3) +
                    GetAddressComponent(Line4) +
                    GetAddressComponent(Town) +
                    GetAddressComponent(Postcode);
                // Remove trailing comma
                if (ret.EndsWith(", "))
                {
                    ret = ret.Remove(ret.Length - 2, 2);
                }
                return ret;
            }
        }
        private string GetAddressComponent(string component)
        {
            return string.IsNullOrWhiteSpace(component) ? "" : component + ", ";
        }

        [JsonProperty("UPRN")]
        public long UPRN { get; set; }

        //Extra fields for Address detailed
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string AddressKey { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public int? USRN { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public long? ParentUPRN { get; set; } //nullable
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string AddressStatus { get; set; } //1 = "Approved", 3 = "Alternative", 5 = "Candidate", 6 = "Provisional", 7 = "Rejected External",  8 = "Historic", 9 = "Rejected Internal"
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string UnitName { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string UnitNumber { get; set; } //string because can be e.g. "1a"
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string BuildingName { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string BuildingNumber { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string Locality { get; set; } //for NLPG results; should be null in results for LLPG
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string Gazetteer { get; set; } //“hackney” or “national”
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string CommercialOccupier { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string Ward { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string UsageDescription { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string UsagePrimary { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string UsageCode { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public string PlanningUseClass { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public bool? PropertyShell { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public bool? OutOfBoroughAddress { get; set; } //for LLPG results; should be null in results for NLPG
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public double? Easting { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public double? Northing { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public double? Longitude { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public double? Latitude { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public int? AddressStartDate { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public int? AddressEndDate { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public int? AddressChangeDate { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public int? PropertyStartDate { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public int? PropertyEndDate { get; set; }
        /// <summary>
        /// Only included if format query parameter is set to detailed.
        /// </summary>
        public int? PropertyChangeDate { get; set; }
    }
}
