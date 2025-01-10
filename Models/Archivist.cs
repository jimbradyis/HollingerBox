using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HollingerBox.Models
{
    [Table("Archivist")]
    public class Archivist
    {
        [Key]
        [Column("Ric")]
        [StringLength(15)]
        public string Ric { get; set; } // PK, NOT NULL

        [StringLength(25)]
        public string First { get; set; }

        [StringLength(50)]
        public string Last { get; set; }

        [Column("Logged in")]
        public bool? LoggedIn { get; set; }  // Yes/No => bool?

        [StringLength(12)]
        public string Password { get; set; }
    }
}