using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddressesAPI.Infrastructure
{
    [Table("hackney_xref", Schema = "dbo")]
    public class CrossReference
    {
        [Column("xref_key")]
        [Key]
        [MaxLength(14)]
        public string CrossRefKey { get; set; }

        [Column("uprn")]
        public long UPRN { get; set; }

        [Column("xref_code")]
        [MaxLength(6)]
        public string Code { get; set; }

        [Column("xref_name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("xref_value")]
        [MaxLength(50)]
        public string Value { get; set; }

        [Column("xref_end_date")]
        public DateTime? EndDate { get; set; }
    }
}
