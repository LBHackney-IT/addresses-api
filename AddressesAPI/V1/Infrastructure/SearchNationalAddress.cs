using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AddressesAPI.V1.Infrastructure
{
    [Table("national_address")]
    public class SearchNationalAddress
    {
        [Column("lpi_key")]
        [Key]
        public string AddressKey { get; set; }

        [Column("lpi_logical_status")]
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

        [Column("usage_description")]
        public string UsageDescription { get; set; }

        [Column("usage_primary")]
        public string UsagePrimary { get; set; }

        [Column("property_shell")]
        public bool PropertyShell { get; set; }

        [Column("easting")]
        public double Easting { get; set; }

        [Column("northing")]
        public double Northing { get; set; }

        [Column("unit_number")]
        public string UnitNumber { get; set; }

        [Column("building_number")]
        public string BuildingNumber { get; set; }

        [Column("street_description")]
        public string Street { get; set; }

        [Column("locality")]
        public string Locality { get; set; }

        [Column("ward")]
        public string Ward { get; set; }

        [Column("town")]
        public string Town { get; set; }

        [Column("postcode")]
        public string Postcode { get; set; }

        [Column("planning_use_class")]
        public string PlanningUseClass { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("gazetteer")]
        public string Gazetteer { get; set; }

        [Column("line1")]
        public string Line1 { get; set; }

        [Column("line2")]
        public string Line2 { get; set; }

        [Column("line3")]
        public string Line3 { get; set; }

        [Column("line4")]
        public string Line4 { get; set; }
    }
}

