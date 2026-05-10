using Microsoft.AspNetCore.Mvc;

namespace Pillowfy.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Profil()
        {
            return View();
        }
        public IActionResult NewReservation()
        {
            return View();
        }
    
        public IActionResult ListeHotels()
        {
            return View();
        }
        public IActionResult ListeReservations()
        {
            return View();
        }
        public IActionResult ListeAvis()
        {
            return View();
        }
        public IActionResult ModifierReservation()
        {
            return View();
        }
        public IActionResult Fidelite()
        {
            return View();
        }
        public IActionResult Factures()
        {
            return View();
        }
        public IActionResult Services()
        {
            return View();
        }
        public IActionResult Sejours()
        {
            return View();
        }

    }
}