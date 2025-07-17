using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Cinema_ETickets.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartRepository _cartRepository;
        private readonly ApplicationDbContext _context;
        private readonly IOrderRepository _orderRepository;

        public CartController(UserManager<ApplicationUser> userManager, ICartRepository cartRepository,
            ApplicationDbContext context,IOrderRepository orderRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _context = context;
            _orderRepository = orderRepository;
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

        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Movie]);

                if (carts is not null)
                {

                    await _orderRepository.CreateAsync(new()
                    {
                        ApplicationUserId = user.Id,
                        Date = DateTime.UtcNow,
                        OrderStatus = OrderStatus.pending,
                        PaymentMethod = PaymentMethod.Visa,
                        TotalPrice = carts.Sum(e => e.Movie.Price * e.Count)
                    });

                    var order = (await _orderRepository.GetAsync(e => e.ApplicationUserId == user.Id)).OrderBy(e => e.Id).LastOrDefault();

                    if (order is null)
                        return BadRequest();

                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                        SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Success?orderId={order.Id}",
                        CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Cancel",
                    };


                    foreach (var item in carts)
                    {
                        options.LineItems.Add(new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "egp",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Movie.Name,
                                    Description = item.Movie.Description,
                                },
                                UnitAmount = (long)item.Movie.Price * 100,
                            },
                            Quantity = item.Count,
                        });
                    }


                    var service = new SessionService();
                    var session = service.Create(options);
                    order.SessionId = session.Id;
                    await _orderRepository.CommitAsync();
                    return Redirect(session.Url);

                }
                return NotFound();
            }
            return NotFound();


        }
    }
}
