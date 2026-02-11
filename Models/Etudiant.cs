using System.ComponentModel.DataAnnotations;

namespace Gestion_Universitaire.Models
{
    public class Etudiant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le matricule est obligatoire")]
        [StringLength(20)]
        public string Matricule { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Telephone { get; set; }

        public DateTime DateNaissance { get; set; }

        [StringLength(200)]
        public string? Adresse { get; set; }

        public DateTime DateInscription { get; set; } = DateTime.Now;

        public bool Actif { get; set; } = true;

        // Relations
        public virtual ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}