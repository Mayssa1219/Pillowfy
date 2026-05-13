using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.Enums;
using Pillowfy.Models;
using Pillowfy.Services;

namespace Pillowfy.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly PilloWfyDbContext _context;
        private readonly StatistiqueService _statistiqueService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            PilloWfyDbContext context,
            StatistiqueService statistiqueService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _statistiqueService = statistiqueService;
        }

        // ── GET /Admin/Login ──────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated && User.IsInRole("Admin"))
                return RedirectToAction("Dashboard");

            return View();
        }

        // ── POST /Admin/Login ─────────────────────────────────────────
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Veuillez remplir tous les champs.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.Error = "Identifiants incorrects.";
                return View();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                ViewBag.Error = "Accès refusé. Espace réservé aux administrateurs.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, password, isPersistent: false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ViewBag.Error = "Mot de passe incorrect.";
                return View();
            }

            return RedirectToAction("Dashboard");
        }

        // ── GET /Admin/Logout ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ── GET /Admin/Dashboard ──────────────────────────────────────
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _statistiqueService.GetDashboardAsync();
            return View(dashboard);
        }

        // ── GET /Admin/ManageUsers ────────────────────────────────────
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersWithRoles = new List<(ApplicationUser User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add((user, roles));
            }

            ViewBag.UsersWithRoles = usersWithRoles;
            return View();
        }

        // ── POST /Admin/ToggleUser ────────────────────────────────────
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                await _userManager.SetLockoutEndDateAsync(user, null);
            else
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            return RedirectToAction("Users");
        }

        // ── GET /Admin/ManageHotels ───────────────────────────────────
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> Hotels()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Owner)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return View(hotels);
        }

        // ── POST /Admin/ToggleHotel ───────────────────────────────────
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHotel(int hotelId)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);
            if (hotel == null)
                return NotFound();

            hotel.IsActive = !hotel.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction("Hotels");
        }

        // ── GET /Admin/Statistiques ───────────────────────────────────
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> Statistiques()
        {
            var dashboard = await _statistiqueService.GetDashboardAsync();
            return View(dashboard);
        }

        // ── POST /Admin/DeleteUser ────────────────────────────────────────
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Sécurité : on ne peut pas supprimer un Admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                TempData["Error"] = "Impossible de supprimer un administrateur.";
                return RedirectToAction("Users");
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = $"Utilisateur {user.Email} supprimé avec succès.";
            return RedirectToAction("Users");
        }

        // ── POST /Admin/DeleteHotel ───────────────────────────────────────
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHotel(int hotelId)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Chambres)           // ← Charge les chambres liées
                .ThenInclude(c => c.Reservations)   // ← Charge les réservations liées
                .FirstOrDefaultAsync(h => h.Id == hotelId);

            if (hotel == null) return NotFound();

            // ⚠️ Vérification : ne pas supprimer si des réservations actives existent
            var hasActiveReservations = hotel.Chambres
                .SelectMany(c => c.Reservations)
                .Any(r => r.Status == ReservationStatus.Confirmed
                       && r.CheckOut >= DateTime.UtcNow);

            if (hasActiveReservations)
            {
                TempData["Error"] = $"Impossible de supprimer \"{hotel.Name}\" : des réservations actives existent.";
                return RedirectToAction("Hotels");
            }

            // 🔥 SUPPRESSION PHYSIQUE (cascade sur chambres si configuré)
            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Hôtel \"{hotel.Name}\" supprimé définitivement.";
            return RedirectToAction("Hotels");
        }

        // ── GET /Admin/Reservations ───────────────────────────────────────
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> Reservations(string? status)
        {
            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Chambre)
                    .ThenInclude(c => c.Hotel)
                .AsQueryable();

            // ✅ CORRIGÉ : Parse l'enum au lieu de ToString()
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, out var statusEnum))
                query = query.Where(r => r.Status == statusEnum);

            var reservations = await query
                .OrderByDescending(r => r.CheckIn)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            return View(reservations);
        }

        // ── Changer le rôle d'un utilisateur (Customer ↔ Owner) ──
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var allowedRoles = new[] { "Customer", "Owner" };
            if (!allowedRoles.Contains(newRole))
            {
                TempData["Error"] = "Rôle invalide.";
                return RedirectToAction("Users");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Sécurité : pas de changement sur un Admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Impossible de modifier le rôle d'un administrateur.";
                return RedirectToAction("Users");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = $"Rôle de {user.Email} changé en {newRole}.";
            return RedirectToAction("Users");
        }

        // ── Modération des avis ──
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> Avis()
        {
            var avis = await _context.Avis
                .Include(a => a.User)
                .Include(a => a.Hotel)
                .OrderByDescending(a => a.DateAvis)
                .ToListAsync();

            return View(avis);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvis(int avisId)
        {
            var avis = await _context.Avis.FindAsync(avisId);
            if (avis == null) return NotFound();

            _context.Avis.Remove(avis);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Avis supprimé.";
            return RedirectToAction("Avis");
        }
    }
}