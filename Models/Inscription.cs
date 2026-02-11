using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_Universitaire.Models
{
    public class Inscription
    {
        public int Id { get; set; }

        public int EtudiantId { get; set; }

        [ForeignKey("EtudiantId")]
        public virtual Etudiant? Etudiant { get; set; }

        public int CoursId { get; set; }

        [ForeignKey("CoursId")]
        public virtual Cours? Cours { get; set; }

        public DateTime DateInscription { get; set; } = DateTime.Now;

        public string AnneeScolaire { get; set; } = DateTime.Now.Year.ToString();

        public string Statut { get; set; } = "Inscrit"; // Inscrit, Validé, Abandonné
    }
}
