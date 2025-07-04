using Microsoft.AspNetCore.Mvc;

namespace Cinema_ETickets.Areas.Identity
{
    public class AccountController : Controller
    {
        public IActionResult Register()
        {
            return View();
        }
    }
}
