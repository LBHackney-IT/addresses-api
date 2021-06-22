using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V2.Boundary.Requests
{
    /// <summary>
    /// SearchAddressRequest V1
    /// Validated by Validate Method
    /// </summary>
    public class SearchAddressRequest
    {
        public SearchAddressRequest()
        {
            AddressStatus = "approved";
        }

        ///<summary>
        /// Query the address text across all address lines. The query will return partial matches and accept some misspelling errors
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Postcode partial match i.e. "E8 4" will return addresses that have a postcode starting with E84**
        /// (Whitespace is removed automatically)
        /// </summary>
        public string Postcode { get; set; }

        /// <summary>
        /// Building number search
        /// </summary>
        [FromQuery(Name = "building_number")]
        public string BuildingNumber { get; set; }

        /// <summary>
        /// Wildcard street name search
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Hackney Borough/Hackney Gazetteer/National (Defaults to Hackney Borough)
        /// </summary>
        [FromQuery(Name = "address_scope")]
        public string AddressScope { get; set; } = GlobalConstants.AddressScope.HackneyBorough.ToString();

        /// <summary>
        /// Filter by UPRN (unique property reference number - unique identifier of the BLPU (Basic Land and Property Unit); a UPRN can have more than one LPI/address. )
        /// </summary>
        public long? UPRN { get; set; }

        /// <summary>
        /// Filter by USRN (unique street reference number - uniquely identifies streets)
        /// </summary>
        public int? USRN { get; set; }

        /// <summary>
        /// Description of the primary usage, can be:
        /// Commercial
        /// Dual Use
        /// Features
        /// Land
        /// Military
        /// Object of Interest
        /// Parent Shell
        /// Residential
        /// Unclassified
        /// ALL (default)
        /// </summary>
        [FromQuery(Name = "usage_primary")]
        public string UsagePrimary { get; set; }

        /// <summary>
        /// Identifies land and property usage according to this system of classification: https://www.geoplace.co.uk/documents/10181/38204/Appendix+C+-+Classifications/ ; this is a textual description
        /// </summary>
        [FromQuery(Name = "usage_code")]
        public string UsageCode { get; set; }

        /// <summary>
        /// Allows a switch between simple and detailed address
        /// </summary>
        public string Format { get; set; } = "Simple";

        /// <summary>
        /// Allows switch between address statuses:
        /// Alternative,
        /// Approved (Default),
        /// Historical,
        /// Provisional
        /// </summary>
        [FromQuery(Name = "address_status")]
        public string AddressStatus { get; set; }

        /// <summary>
        /// Whether or not to include property shells in the results.
        /// </summary>
        [FromQuery(Name = "include_property_shells")]
        public bool IncludePropertyShells { get; set; } = false;

        /// <summary>
        /// Filter addresses by a specific cross reference. Must be used together with `cross_ref_value`.
        /// </summary>
        [FromQuery(Name = "cross_ref_code")]
        public string CrossRefCode { get; set; }

        /// <summary>
        /// Filter addresses by a specific cross reference. Must be used together with `cross_ref_code`.
        /// </summary>
        [FromQuery(Name = "cross_ref_value")]
        public string CrossRefValue { get; set; }

        /// <summary>
        /// Filter addresses by those which have been modified since the provided date. Date should provided in the format YYYY-MM-DD.
        /// </summary>
        [FromQuery(Name = "modified_since")]
        public string ModifiedSince { get; set; }

        /// <summary>
        /// Page defaults to 1 as paging is 1 index based not 0 index based
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// PageSize defaults to 50 if not provided
        /// </summary>
        [FromQuery(Name = "page_size")]
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// List of fields passed in as part of the request
        /// </summary>
        public List<string> RequestFields { get; set; }
    }
}
