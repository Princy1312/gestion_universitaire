using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_Universitaire.Data;
using Gestion_Universitaire.Models;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;


namespace Gestion_Universitaire.Controllers
{
    [Authorize]
    public class CoursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cours = await _context.Cours
                .Include(c => c.Professeur)
                .OrderBy(c => c.Intitule)
                .ToListAsync();
            return View(cours);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cours = await _context.Cours
                .Include(c => c.Professeur)
                .Include(c => c.Inscriptions)
                    .ThenInclude(i => i.Etudiant)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cours == null) return NotFound();

            return View(cours);
        }

        public IActionResult Create()
        {
            ViewData["ProfesseurId"] = new SelectList(_context.Professeurs, "Id", "Nom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Intitule,Description,Credits,VolumeHoraire,ProfesseurId")] Cours cours, IFormFile? PdfFile)
        {
            if (ModelState.IsValid)
            {
                if (PdfFile != null && PdfFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pdfs");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + PdfFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await PdfFile.CopyToAsync(fileStream);
                    }

                    cours.PdfPath = "/uploads/pdfs/" + uniqueFileName;
                }

                _context.Add(cours);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cours créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProfesseurId"] = new SelectList(_context.Professeurs, "Id", "Nom", cours.ProfesseurId);
            return View(cours);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cours = await _context.Cours.FindAsync(id);
            if (cours == null) return NotFound();

            ViewData["ProfesseurId"] = new SelectList(_context.Professeurs, "Id", "Nom", cours.ProfesseurId);
            return View(cours);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Intitule,Description,Credits,VolumeHoraire,ProfesseurId,Actif")] Cours cours)
        {
            if (id != cours.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cours);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cours modifié avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoursExists(cours.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProfesseurId"] = new SelectList(_context.Professeurs, "Id", "Nom", cours.ProfesseurId);
            return View(cours);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cours = await _context.Cours.FindAsync(id);
            if (cours == null) return NotFound();

            return View(cours);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cours = await _context.Cours.FindAsync(id);
            if (cours != null)
            {
                _context.Cours.Remove(cours);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cours supprimé avec succès.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CoursExists(int id)
        {
            return _context.Cours.Any(e => e.Id == id);
        }
    }
}
