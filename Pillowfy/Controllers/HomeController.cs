using Microsoft.AspNetCore.Mvc;

namespace Pillowfy.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
