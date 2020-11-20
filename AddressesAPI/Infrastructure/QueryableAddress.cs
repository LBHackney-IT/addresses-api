using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nest;

namespace AddressesAPI.Infrastructure
{
    //BLPU: Basic Land and Property Unit
    //LPI: Local Property Identifier
    public class QueryableAddress
    {
        public string id { get; set; }

        [MaxLength(14)]
        public string AddressKey { get; set; }

        [MaxLength(18)]
        public string AddressStatus { get; set; }

        public int AddressChangeDate { get; set; }

        public int? USRN { get; set; }

        public long UPRN { get; set; }

        public long? ParentUPRN { get; set; }

        [MaxLength(4)]
        public string UsageCode { get; set; }

        [MaxLength(160)]
        public string UsagePrimary { get; set; }

        public bool PropertyShell { get; set; }

        [MaxLength(17)]
        public string BuildingNumber { get; set; }

        [MaxLength(100)]
        public string Street { get; set; }

        [MaxLength(100)]
        public string Town { get; set; }

        [MaxLength(8)]
        public string Postcode { get; set; }

        public bool OutOfBoroughAddress { get; set; }

        [MaxLength(8)]
        public string Gazetteer { get; set; }

        [MaxLength(200)]
        public string Line1 { get; set; }

        [MaxLength(200)]
        public string Line2 { get; set; }

        [MaxLength(200)]
        public string Line3 { get; set; }

        [MaxLength(100)]
        public string Line4 { get; set; }

        public short? PaonStartNumber { get; set; }
        public string UnitNumber { get; set; }
        public string UnitName { get; set; }
    }
}
