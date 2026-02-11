using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_Universitaire.Data;
using Gestion_Universitaire.Models;
using System.Text;


namespace Gestion_Universitaire.Controllers
{
    [Authorize]
    public class EtudiantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EtudiantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var etudiants = _context.Etudiants.Where(e => e.Actif);

            if (!string.IsNullOrEmpty(searchString))
            {
                etudiants = etudiants.Where(e =>
                    e.Nom.Contains(searchString) ||
                    e.Prenom.Contains(searchString) ||
                    e.Matricule.Contains(searchString));
            }

            return View(await etudiants.OrderByDescending(e => e.DateInscription).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var etudiant = await _context.Etudiants
                .Include(e => e.Inscriptions)
                    .ThenInclude(i => i.Cours)
                .Include(e => e.Notes)
                    .ThenInclude(n => n.Cours)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etudiant == null) return NotFound();

            return View(etudiant);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Matricule,Nom,Prenom,Email,Telephone,DateNaissance,Adresse")] Etudiant etudiant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(etudiant);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Étudiant créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            return View(etudiant);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var etudiant = await _context.Etudiants.FindAsync(id);
            if (etudiant == null) return NotFound();

            return View(etudiant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Matricule,Nom,Prenom,Email,Telephone,DateNaissance,Adresse,DateInscription,Actif")] Etudiant etudiant)
        {
            if (id != etudiant.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(etudiant);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Étudiant modifié avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EtudiantExists(etudiant.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(etudiant);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var etudiant = await _context.Etudiants.FirstOrDefaultAsync(e => e.Id == id);
            if (etudiant == null) return NotFound();

            return View(etudiant);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var etudiant = await _context.Etudiants.FindAsync(id);
            if (etudiant != null)
            {
                etudiant.Actif = false;
                _context.Update(etudiant);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Étudiant désactivé avec succès.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EtudiantExists(int id)
        {
            return _context.Etudiants.Any(e => e.Id == id);
        }
    }
}
