using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HollingerBox.Models
{
    [Table("Archive")]
    public class Archive
    {
        [Key]
        [Column("HASC Key")]
        [StringLength(30)]
        public string HascKey { get; set; }   // PK, NOT NULL in SQLite

        [Column("Subcommittee")]
        [StringLength(25)]
        [Required]
        public string Subcommittee { get; set; }  // references Inquiry.Subcommittee, NOT NULL

        [Column("Archive No")]
        [Required]
        public int ArchiveNo { get; set; }    // NOT NULL

        [Required]
        public int Congress { get; set; }     // references Congress.CongressNo, NOT NULL

        public bool? Classified { get; set; } // Yes/No => bool?

        [Required]
        [StringLength(30)]
        public string Status { get; set; }    // NOT NULL

        [Column("Hollinger Box Key")]
        [StringLength(10)]
        public string HollingerBoxKey { get; set; }

        public bool? Printed { get; set; }    // Yes/No => bool?

        public string Note { get; set; }      // long text -> no length limit

        [Column("Box Label without congress")]
        public short? BoxLabelWithoutCongress { get; set; }

        [Column("Box Label problem")]
        public bool? BoxLabelProblem { get; set; } // bool?

        public bool? DocFound { get; set; }

        [StringLength(50)]
        public string Label1 { get; set; }

        [StringLength(50)]
        public string Label2 { get; set; }

        [StringLength(50)]
        public string Label3 { get; set; }

        [StringLength(50)]
        public string Label4 { get; set; }

        // -------------- OPTIONAL NAVIGATION PROPERTIES --------------
        // [ForeignKey(nameof(Subcommittee))]
        // public Inquiry Inquiry { get; set; }

        // [ForeignKey(nameof(Congress))]
        // public Congress CongressRef { get; set; }
    }
}
