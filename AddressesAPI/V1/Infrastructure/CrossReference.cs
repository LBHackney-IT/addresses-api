using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AddressesAPI.V1.Infrastructure
{
    [Table("hackney_xref")]
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
