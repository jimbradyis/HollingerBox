using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HollingerBox.Models
{
    [Table("Congress")]
    public class Congress
    {
        [Key]
        [Column("CongressNo")]
        public int CongressNo { get; set; }  // PK (NOT NULL)

        [StringLength(10)]
        public string Years { get; set; }

        // "Default" is a reserved word in many contexts, but we’ll keep it.
        // You might rename the property (e.g. IsDefault) but keep [Column("Default")] 
        [Column("Default")]
        public bool? DefaultVal { get; set; }  // Yes/No in Access => bool?

        [Column("Year Label")]
        [StringLength(10)]
        public string YearLabel { get; set; }

        [StringLength(50)]
        public string Committee { get; set; }
    }
}