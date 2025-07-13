using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cinema_ETickets.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartRepository _cartRepository;
        private readonly ApplicationDbContext _context;

        public CartController(UserManager<ApplicationUser> userManager, ICartRepository cartRepository,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _context = context;
        }

        public async Task<IActionResult> AddToCart(int MovieId, int Count)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is not null)
            {
                var movie = _context.movies.Find(MovieId);
                if (movie is not null)
                {
                    if (movie.TicketsQuantity > 0)
                    {
                        var ticketsInCart = await _cartRepository.GetOneAsync(e =>
                        e.ApplicationUserId == user.Id && e.MovieId == MovieId);
                        if (ticketsInCart is not null)
                        {
                            if (ticketsInCart.Count + Count > movie.TicketsQuantity)
                            {
                                TempData["error-notification"] = "Number of tickets exceeds available quantity.";
                                return RedirectToAction("Details", "Home", new { area = "Customer", id = MovieId });
                            }
                            ticketsInCart.Count += Count;
                        }
                        else
                        {
                            await _cartRepository.CreateAsync(new Cart
                            {
                                MovieId = MovieId,
                                Count = Count,
                                ApplicationUser = user
                            });
                        }
                        await _cartRepository.CommitAsync();
                        TempData["success-notification"] = "Add To Cart Successfully";
                        return RedirectToAction("Index", "Home");
                    }

                    TempData["error-notification"] = "invalid Count";
                    return RedirectToAction("Details", "home", new { area = "Customer", id = MovieId });
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> Index()
        {

            var user = await _userManager.GetUserAsync(User);

            if (user is not null)
            {
                var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id,
                    includes: [m => m.Movie]);
                var TotalPrice = carts.Sum(x => x.Movie.Price * x.Count);
                ViewBag.TotalPrice = TotalPrice;
                return View(carts);
            }
            return NotFound();
        }

        public async Task<IActionResult> IncrementCount(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                var ticketsInCart = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == movieId);
                if (ticketsInCart is not null)
                {
                    ticketsInCart.Count++;
                    _context.SaveChanges();

                    return RedirectToAction("Index");
                }
                return NotFound();
            }
            return NotFound();

        }

        public async Task<IActionResult> DecrementCount(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                var ticketsInCart = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == movieId);
                if (ticketsInCart is not null)
                {
                    if (ticketsInCart.Count > 1)
                    {
                        ticketsInCart.Count--;
                        _context.SaveChanges();
                    }

                    return RedirectToAction("Index");
                }
                return NotFound();
            }
            return NotFound();

        }

        public async Task<IActionResult> Delete(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                var ticketsInCart = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == movieId);
                if (ticketsInCart is not null)
                {
                    await _cartRepository.DeleteAsync(ticketsInCart);
                    await _cartRepository.CommitAsync();

                    return RedirectToAction("Index");
                }
                return NotFound();
            }
            return NotFound();

        }
    }
}
