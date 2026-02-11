using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_Universitaire.Models
{
    public class Cours
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Intitule { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, 30)]
        public int Credits { get; set; }

        [Range(1, 100)]
        public int VolumeHoraire { get; set; }

        public int? ProfesseurId { get; set; }

        [ForeignKey("ProfesseurId")]
        public virtual Professeur? Professeur { get; set; }

        public bool Actif { get; set; } = true;

        public string? PdfPath { get; set; }

        // Relations
        public virtual ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
