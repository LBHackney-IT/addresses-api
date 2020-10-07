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
        public string CrossRefKey { get; set; }

        [Column("uprn")]
        public long UPRN { get; set; }

        [Column("xref_code")]
        public string Code { get; set; }

        [Column("xref_name")]
        public string Name { get; set; }

        [Column("xref_value")]
        public string Value { get; set; }

        [Column("xref_end_date")]
        public DateTime? EndDate { get; set; }
    }
}
