using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HollingerBox.Models
{
    [Table("Docs")]
    public class Docs
    {
        [Key]
        [Column("Key")]
        // Indicate that this is an auto-increment PK in SQLite
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Key { get; set; }

        [Column("Doc Descrip")]
        [Required]
        public string DocDescrip { get; set; } // long text, Required

        [StringLength(15)]
        public string Action { get; set; }

        [Column("HASC Key")]
        [StringLength(30)]
        public string HascKey { get; set; }  // references Archive

        [Column("User ID")]
        [StringLength(12)]
        public string UserID { get; set; }   // references Archivist

        // -------------- OPTIONAL NAVIGATION PROPERTIES --------------
        // [ForeignKey(nameof(HascKey))]
        // public Archive Archive { get; set; }

        // [ForeignKey(nameof(UserID))]
        // public Archivist Archivist { get; set; }
    }
}