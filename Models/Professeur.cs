using System.ComponentModel.DataAnnotations;

namespace Gestion_Universitaire.Models
{
    public class Professeur
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Matricule { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Telephone { get; set; }

        [StringLength(100)]
        public string? Specialite { get; set; }

        [StringLength(50)]
        public string? Grade { get; set; }

        public DateTime DateEmbauche { get; set; } = DateTime.Now;

        public bool Actif { get; set; } = true;

        // Relations
        public virtual ICollection<Cours> Cours { get; set; } = new List<Cours>();
    }
}
