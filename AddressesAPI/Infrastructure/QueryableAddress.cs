using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nest;

namespace AddressesAPI.Infrastructure
{
    //BLPU: Basic Land and Property Unit
    //LPI: Local Property Identifier
    public class QueryableAddress
    {
        [MaxLength(14)]
        [Text(Name = "lpi_key")]
        public string AddressKey { get; set; }

        [MaxLength(18)]
        [Text(Name = "lpi_logical_status")]
        public string AddressStatus { get; set; }

        [Text(Name = "lpi_last_update_date")]
        public int AddressChangeDate { get; set; }

        [Text(Name = "usrn")]
        public int? USRN { get; set; }

        [Text(Name = "uprn")]
        public long UPRN { get; set; }

        [Text(Name = "parent_uprn")]
        public long? ParentUPRN { get; set; }

        [MaxLength(4)]
        [Text(Name = "blpu_class")]
        public string UsageCode { get; set; }

        [MaxLength(160)]
        [Text(Name = "usage_primary")]
        public string UsagePrimary { get; set; }

        [Text(Name = "property_shell")]
        public bool PropertyShell { get; set; }

        [MaxLength(17)]
        [Text(Name = "building_number")]
        public string BuildingNumber { get; set; }

        [MaxLength(100)]
        [Text(Name = "street_description")]
        public string Street { get; set; }

        [MaxLength(100)]
        [Text(Name = "town")]
        public string Town { get; set; }

        [MaxLength(8)]
        public string Postcode { get; set; }

        [Text(Name = "neverexport")]
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

        [MaxLength(700)]
        [Text(Name = "full_address")]
        public string FullAddress { get; set; }

        [Text(Name = "paon_start_num")]
        public short? PaonStartNumber { get; set; }
        [Text(Name = "unit_number")]
        public string UnitNumber { get; set; }
        [Text(Name = "sao_text")]
        public string UnitName { get; set; }

        [Text(Name = "blpu_last_update_date")]
        public int? PropertyChangeDate { get; set; }
    }
}
