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
        /// LOCAL/NATIONAL/BOTH (Defaults to LOCAL)
        /// </summary>
        public string Gazetteer { get; set; } = GlobalConstants.Gazetteer.Both.ToString();

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
        public string usagePrimary { get; set; }

        /// <summary>
        /// Identifies land and property usage according to this system of classification: https://www.geoplace.co.uk/documents/10181/38204/Appendix+C+-+Classifications/ ; this is a textual description
        /// </summary>
        public string usageCode { get; set; }

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
        ///  	wether or not out of borough addresses
        ///that are present in the local gazetteer
        ///for services reasons should be returned.
        ///If yes, the local gazetteer version takes
        ///precedence over the national gazetteer
        ///version.
        /// </summary>
        [FromQuery(Name = "out_of_borough")]
        public bool OutOfBoroughAddress { get; set; } = true;

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
