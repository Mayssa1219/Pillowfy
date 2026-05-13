using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pillowfy.Controllers
{
    [Authorize(Roles = "Customer")]
    [Route("Client")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ClientController : Controller
    {
        [Route("Dashboard")]

        public IActionResult Dashboard() => View();

        [Route("Profil")]
        public IActionResult Profil() => View();

        [Route("ListeHotels")]
        public IActionResult ListeHotels() => View("ListeHotels");

        [Route("ListeReservations")]
        public IActionResult ListeReservations() => View("ListeReservations");

        [Route("NewReservation")]
        public IActionResult NewReservation([FromQuery] int? chambreId, [FromQuery] int? points) => View();

        
        [Route("ModifierReservation")]
        public IActionResult ModifierReservation([FromQuery] int id) => View();

        [Route("ListeAvis")]
        public IActionResult ListeAvis() => View("ListeAvis");

        [Route("Fidelite")]
        public IActionResult Fidelite() => View();

        [Route("Factures")]
        public IActionResult Factures() => View();

        [Route("Services")]
        public IActionResult Services() => View();

        [Route("Sejours")]
        public IActionResult Sejours() => View();
    }
}