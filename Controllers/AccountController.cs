using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Gestion_Universitaire.Models;
using Gestion_Universitaire.Models.ViewModels;
using Gestion_Universitaire.Services;


namespace Gestion_Universitaire.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Génération du code 2FA
                    string code = new Random().Next(100000, 999999).ToString();
                    user.Code2FA = code;
                    user.Expiration2FA = DateTime.Now.AddMinutes(5);
                    await _userManager.UpdateAsync(user);

                    // Envoi du code par email
                    await _emailService.EnvoyerCode(user.Email, code);

                    // Stocker l'ID utilisateur en session pour la vérification
                    HttpContext.Session.SetString("UserId2FA", user.Id);

                    // Rediriger vers la page de vérification 2FA
                    return RedirectToAction("Verify2FA");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }



        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        // Connexion directe sans 2FA pour la connexion
                        await _signInManager.SignInAsync(user, model.RememberMe);
                        return RedirectToLocal(returnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        return View("Lockout");
                    }
                }

                ModelState.AddModelError(string.Empty, "Tentative de connexion invalide.");
            }

            return View(model);
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile photo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (photo == null || photo.Length == 0)
            {
                TempData["ErrorMessage"] = "Aucun fichier sélectionné.";
                return RedirectToAction(nameof(Profile));
            }

            // Vérifier le type de fichier
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                TempData["ErrorMessage"] = "Type de fichier non autorisé. Veuillez télécharger une image (JPG, JPEG, PNG ou GIF).";
                return RedirectToAction(nameof(Profile));
            }

            // Créer le dossier de stockage s'il n'existe pas
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile-photos");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Générer un nom de fichier unique
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Enregistrer le fichier
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(fileStream);
            }

            // Mettre à jour le chemin de la photo de profil de l'utilisateur
            var webPath = $"/uploads/profile-photos/{uniqueFileName}";
            
            // Supprimer l'ancienne photo si elle existe
            if (!string.IsNullOrEmpty(user.PhotoProfil))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PhotoProfil.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }
            
            user.PhotoProfil = webPath;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la mise à jour du profil.";
                // Supprimer l'image téléchargée en cas d'échec
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            return RedirectToAction(nameof(Profile));
        }

        // VERIFY 2FA GET
        [HttpGet]
        public IActionResult Verify2FA()
        {
            var userId = HttpContext.Session.GetString("UserId2FA");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // VERIFY 2FA POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify2FA(string code)
        {
            var userId = HttpContext.Session.GetString("UserId2FA");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Utilisateur introuvable.");
                return View();
            }

            if (user.Code2FA == code && user.Expiration2FA > DateTime.Now)
            {
                // Réinitialiser le code 2FA après utilisation
                user.Code2FA = null;
                user.Expiration2FA = null;
                await _userManager.UpdateAsync(user);

                // Supprimer l'ID utilisateur de la session
                HttpContext.Session.Remove("UserId2FA");

                // Connecter l'utilisateur
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Code de vérification invalide ou expiré.");
            return View();
        }

        // RESEND 2FA CODE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resend2FACode()
        {
            var userId = HttpContext.Session.GetString("UserId2FA");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Session expirée. Veuillez réessayer l'inscription." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Utilisateur introuvable." });
            }

            // Générer un nouveau code 2FA
            string newCode = new Random().Next(100000, 999999).ToString();
            user.Code2FA = newCode;
            user.Expiration2FA = DateTime.Now.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            // Envoyer le nouveau code par email
            try
            {
                await _emailService.EnvoyerCode(user.Email, newCode);
                return Json(new { success = true, message = "Un nouveau code a été envoyé à votre adresse email." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de l'envoi du code. Veuillez réessayer." });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            // Vérifier que l'ID fourni correspond à l'utilisateur connecté
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id != id)
            {
                TempData["ErrorMessage"] = "Opération non autorisée.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Supprimer d'abord l'utilisateur de la base de données
                var result = await _userManager.DeleteAsync(currentUser);
                if (!result.Succeeded)
                {
                    throw new Exception("Échec de la suppression du compte utilisateur.");
                }

                // Déconnecter l'utilisateur
                await _signInManager.SignOutAsync();

                // Supprimer le cookie d'authentification
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                // Supprimer les données de session
                HttpContext.Session.Clear();

                TempData["SuccessMessage"] = "Votre compte a été supprimé avec succès.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Journaliser l'erreur (à implémenter avec un système de journalisation)
                // _logger.LogError(ex, "Erreur lors de la suppression du compte utilisateur");

                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression du compte. Veuillez réessayer ou contacter l'administrateur.";
                return RedirectToAction("Delete");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new Models.ViewModels.EditProfileViewModel
            {
                Id = user.Id,
                Prenom = user.Prenom,
                Nom = user.Nom,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.ViewModels.EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Vérifier si l'email a été modifié
            if (user.Email != model.Email)
            {
                // Vérifier si le nouvel email est déjà utilisé
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError(string.Empty, "Cette adresse email est déjà utilisée.");
                    return View(model);
                }
                
                // Mettre à jour l'email
                user.Email = model.Email;
                user.UserName = model.Email;
                user.EmailConfirmed = false; // L'utilisateur devra confirmer son nouvel email
            }

            // Mettre à jour les autres champs
            user.Prenom = model.Prenom;
            user.Nom = model.Nom;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Si l'email a été modifié, déconnecter l'utilisateur pour qu'il confirme son nouvel email
                if (!user.EmailConfirmed)
                {
                    // Ici, vous pourriez envoyer un email de confirmation
                    // await SendEmailConfirmationAsync(user);
                    await _signInManager.SignOutAsync();
                    TempData["StatusMessage"] = "Votre profil a été mis à jour. Veuillez confirmer votre nouvel email pour vous reconnecter.";
                    return RedirectToAction("Login");
                }

                TempData["StatusMessage"] = "Votre profil a été mis à jour avec succès.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }



        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }

    // ViewModels
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class LoginWith2faViewModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "Le code doit contenir {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Code d'authentification")]
        public string TwoFactorCode { get; set; } = string.Empty;

        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }

        [Display(Name = "Se souvenir de cet appareil")]
        public bool RememberMachine { get; set; }
    }

    public class VerifyRegistration2faViewModel
    {
        [Required]
        [StringLength(6, ErrorMessage = "Le code doit contenir {2} caractères.", MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Le code doit contenir exactement 6 chiffres.")]
        [Display(Name = "Code d'authentification")]
        public string Code { get; set; } = string.Empty;
    }
}
