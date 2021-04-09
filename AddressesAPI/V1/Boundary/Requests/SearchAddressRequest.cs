using System.Collections.Generic;

namespace AddressesAPI.V1.Boundary.Requests
{
    /// <summary>
    /// SearchAddressRequest V1
    /// Validated by Validate Method
    /// </summary>
    public class SearchAddressRequest
    {
        private string _gazetteer;

        public SearchAddressRequest()
        {
            AddressStatus = "approved preferred";
            Gazetteer = GlobalConstants.Gazetteer.Both.ToString();
        }

        /// <summary>
        /// Postcode partial match i.e. "E8 4" will return addresses that have a postcode starting with E84**
        /// (Whitespace is removed automatically)
        /// </summary>
        public string PostCode { get; set; }

        /// <summary>
        /// Building number search
        /// </summary>
        public string BuildingNumber { get; set; }

        /// <summary>
        /// Wildcard street name search
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// LOCAL/NATIONAL/BOTH (Defaults to LOCAL)
        /// </summary>
        public string Gazetteer
        {
            get => _gazetteer;
            set
            {
                //This logic needs reworking
                _gazetteer = value;

                if (_gazetteer == GlobalConstants.Gazetteer.Hackney.ToString())
                {
                    HackneyGazetteerOutOfBoroughAddress = false;
                }
                else if (_gazetteer == GlobalConstants.Gazetteer.Both.ToString())
                {
                    HackneyGazetteerOutOfBoroughAddress = null;
                }
            }
        }

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
        /// Approved Preferred (Default),
        /// Historical,
        /// Provisional
        /// </summary>
        public string AddressStatus { get; set; }

        /// <summary>
        ///  	wether or not out of borough addresses
        ///that are present in the local gazetteer
        ///for services reasons should be returned.
        ///If yes, the local gazetteer version takes
        ///precedence over the national gazetteer
        ///version.
        /// </summary>
        public bool? HackneyGazetteerOutOfBoroughAddress { get; set; }

        /// <summary>
        /// Page defaults to 1 as paging is 1 index based not 0 index based
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// PageSize defaults to 50 if not provided
        /// </summary>
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// List of fields passed in as part of the request
        /// </summary>
        public List<string> RequestFields { get; set; }
    }
}
