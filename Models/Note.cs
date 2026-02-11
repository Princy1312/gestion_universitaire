using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_Universitaire.Models
{
    public class Note
    {
        public int Id { get; set; }

        public int EtudiantId { get; set; }

        [ForeignKey("EtudiantId")]
        public virtual Etudiant? Etudiant { get; set; }

        public int CoursId { get; set; }

        [ForeignKey("CoursId")]
        public virtual Cours? Cours { get; set; }

        [Range(0, 20)]
        public decimal Valeur { get; set; }

        [StringLength(50)]
        public string TypeEvaluation { get; set; } = "Examen"; // Examen, Contrôle, TP, TD

        public DateTime DateEvaluation { get; set; } = DateTime.Now;

        public string? Commentaire { get; set; }
    }
}
