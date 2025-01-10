using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HollingerBox.Models
{
    [Table("Inquiry")]
    public class Inquiry
    {
        [Key]
        [Column("Subcommittee")]
        [StringLength(25)]
        public string Subcommittee { get; set; }  // PK (NOT NULL)

        [Column("Long Name")]
        [StringLength(50)]
        public string LongName { get; set; }      // optional

        [StringLength(12)]
        public string Password { get; set; }      // optional
    }
}