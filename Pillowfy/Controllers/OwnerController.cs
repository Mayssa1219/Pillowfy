using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pillowfy.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Owner")]
    [Route("Owner")]
    public class OwnerController : Controller
    {
        [Route("Dashboard")]
        public IActionResult Dashboard() => View();

        [Route("Hotels")]
        public IActionResult Hotels() => View();
        [Route("Reservations")]
        public IActionResult Reservations() => View();
        [Route("Chambres")]
        public IActionResult Chambres() => View();
        [Route("Avis")]
        public IActionResult Avis() => View();
        [Route("Revenus")]
        public IActionResult Revenus() => View();
        [Route("Paiements")]
        public IActionResult Paiements() => View();
        [Route("Profil")]
        public IActionResult Profil() => View();

    } 
}