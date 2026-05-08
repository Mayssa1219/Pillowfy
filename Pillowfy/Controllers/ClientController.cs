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
    }
}