using Microsoft.AspNetCore.Identity;

namespace Gestion_Universitaire.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public string? PhotoProfil { get; set; }
    }
}
