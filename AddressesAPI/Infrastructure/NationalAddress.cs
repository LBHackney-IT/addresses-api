using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddressesAPI.Infrastructure
{
    //BLPU: Basic Land and Property Unit
    //LPI: Local Property Identifier
    [Table("national_address", Schema = "dbo")]
    public class NationalAddress
    {
        [Column("lpi_key")]
        [MaxLength(14)]
        [Key]
        public string AddressKey { get; set; }

        [Column("lpi_logical_status")]
        [MaxLength(18)]
        public string AddressStatus { get; set; }

        [Column("lpi_start_date")]
        public int AddressStartDate { get; set; }

        [Column("lpi_end_date")]
        public int AddressEndDate { get; set; }

        [Column("lpi_last_update_date")]
        public int AddressChangeDate { get; set; }

        [Column("usrn")]
        public int? USRN { get; set; }

        [Column("uprn")]
        public long UPRN { get; set; }

        [Column("parent_uprn")]
        public long? ParentUPRN { get; set; }

        [Column("blpu_start_date")]
        public int PropertyStartDate { get; set; }

        [Column("blpu_end_date")]
        public int PropertyEndDate { get; set; }

        [Column("blpu_class")]
        [MaxLength(4)]
        public string UsageCode { get; set; }

        [Column("blpu_last_update_date")]
        public int? PropertyChangeDate { get; set; }

        [Column("usage_description")]
        [MaxLength(160)]
        public string UsageDescription { get; set; }

        [Column("usage_primary")]
        [MaxLength(160)]
        public string UsagePrimary { get; set; }

        [Column("property_shell")]
        public bool PropertyShell { get; set; }

        [Column("easting")]
        public double Easting { get; set; }

        [Column("northing")]
        public double Northing { get; set; }

        [Column("unit_number")]
        public int? UnitNumber { get; set; }

        [Column("sao_text")]
        [MaxLength(90)]
        public string UnitName { get; set; }

        [Column("building_number")]
        [MaxLength(17)]
        public string BuildingNumber { get; set; }

        [Column("pao_text")]
        [MaxLength(90)]
        public string BuildingName { get; set; }

        [Column("paon_start_num")]
        public short? PaonStartNumber { get; set; }

        [Column("street_description")]
        [MaxLength(100)]
        public string Street { get; set; }

        [Column("locality")]
        [MaxLength(100)]
        public string Locality { get; set; }

        [Column("ward")]
        [MaxLength(100)]
        public string Ward { get; set; }

        [Column("town")]
        [MaxLength(100)]
        public string Town { get; set; }

        [Column("postcode")]
        [MaxLength(8)]
        public string Postcode { get; set; }

        [Column("postcode_nospace")]
        [MaxLength(8)]
        public string PostcodeNoSpace { get; set; }

        [Column("planning_use_class")]
        [MaxLength(50)]
        public string PlanningUseClass { get; set; }

        [Column("neverexport")]
        public bool NeverExport { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("gazetteer")]
        [MaxLength(8)]
        public string Gazetteer { get; set; }

        [Column("organisation")]
        [MaxLength(100)]
        public string Organisation { get; set; }

        [Column("line1")]
        [MaxLength(200)]
        public string Line1 { get; set; }

        [Column("line2")]
        [MaxLength(200)]
        public string Line2 { get; set; }

        [Column("line3")]
        [MaxLength(200)]
        public string Line3 { get; set; }

        [Column("line4")]
        [MaxLength(100)]
        public string Line4 { get; set; }
    }
}
