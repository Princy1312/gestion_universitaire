using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_Universitaire.Data;
using Gestion_Universitaire.Models;
using System.Text;


namespace Gestion_Universitaire.Controllers
{
    [Authorize]
    public class ProfesseursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfesseursController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Professeurs.OrderBy(p => p.Nom).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var professeur = await _context.Professeurs
                .Include(p => p.Cours)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (professeur == null) return NotFound();

            return View(professeur);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Matricule,Nom,Prenom,Email,Telephone,Specialite,Grade")] Professeur professeur)
        {
            if (ModelState.IsValid)
            {
                _context.Add(professeur);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Professeur créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            return View(professeur);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var professeur = await _context.Professeurs.FindAsync(id);
            if (professeur == null) return NotFound();

            return View(professeur);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Matricule,Nom,Prenom,Email,Telephone,Specialite,Grade,DateEmbauche,Actif")] Professeur professeur)
        {
            if (id != professeur.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(professeur);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Professeur modifié avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfesseurExists(professeur.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(professeur);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var professeur = await _context.Professeurs.FindAsync(id);
            if (professeur == null) return NotFound();

            return View(professeur);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var professeur = await _context.Professeurs
                .Include(p => p.Cours)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (professeur != null)
            {
                // Supprimer les cours associés
                foreach (var cours in professeur.Cours)
                {
                    _context.Cours.Remove(cours);
                }
                _context.Professeurs.Remove(professeur);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Professeur supprimé avec succès.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProfesseurExists(int id)
        {
            return _context.Professeurs.Any(e => e.Id == id);
        }
    }
}
