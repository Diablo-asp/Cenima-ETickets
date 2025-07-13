using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cinema_ETickets.Areas.Customer.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(UserManager<ApplicationUser> userManager,)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> AddToCart(int MovieId, int Count)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is not null)
            {

            }
            return NotFound();
        }
    }
}
