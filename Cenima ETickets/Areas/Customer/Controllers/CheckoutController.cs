using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Cinema_ETickets.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(IOrderRepository orderRepository, ICartRepository cartRepository, IOrderItemRepository orderItemRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _orderItemRepository = orderItemRepository;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _orderRepository.GetOneAsync(e => e.Id == orderId);

            if (order is null)
                return NotFound();

            order.OrderStatus = OrderStatus.Completed;

            var service = new SessionService();
            var session = service.Get(order.SessionId);

            order.PaymentId = session.PaymentIntentId;

            await _orderRepository.CommitAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();

            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Movie]);

            List<OrderItem> orderItems = new();
            foreach (var item in carts)
            {
                orderItems.Add(new()
                {
                    MovieId = item.MovieId,
                    OrderId = orderId,
                    Quantity = item.Count,
                    TotalPrice = (decimal)(item.Movie.Price * item.Count)
                });
                var movie = _context.movies.Find(item.MovieId);
                if(movie is not null)
                {
                    movie.TicketsQuantity -= item.Count;
                }
            }

            await _orderItemRepository.CreateRangeAsync(orderItems);

            foreach (var item in carts)
            {
                await _cartRepository.DeleteAsync(item);
            }

            await _context.SaveChangesAsync();

            var orders = await _orderItemRepository.GetAsync(
                e => e.OrderId == orderId,
                includes: [e => e.Movie]);

            return View(new SuccessVM
            {
                Order = order,
                OrderItems = orderItems.ToList()
            });
        }
    }
}
