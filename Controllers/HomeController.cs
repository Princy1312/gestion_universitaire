using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_Universitaire.Data;
using Gestion_Universitaire.Models;
using System.Text;


namespace Gestion_Universitaire.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalEtudiants = await _context.Etudiants.CountAsync(e => e.Actif),
                TotalProfesseurs = await _context.Professeurs.CountAsync(),
                TotalCours = await _context.Cours.CountAsync(),
                DernierEtudiant = await _context.Etudiants
                    .Where(e => e.Actif)
                    .OrderByDescending(e => e.DateInscription)
                    .FirstOrDefaultAsync()
            };

            return View(viewModel);
        }

        public IActionResult Error()
        {
            return View();
        }
    }

    public class DashboardViewModel
    {
        public int TotalEtudiants { get; set; }
        public int TotalProfesseurs { get; set; }
        public int TotalCours { get; set; }
        public Etudiant? DernierEtudiant { get; set; }
    }
}
