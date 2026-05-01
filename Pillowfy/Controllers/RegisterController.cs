using Microsoft.AspNetCore.Mvc;

namespace Pillowfy.Controllers
{
    public class RegisterController:Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
