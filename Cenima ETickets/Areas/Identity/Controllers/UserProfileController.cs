using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Cinema_ETickets.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly ICartRepository _cartRepository;

        public UserProfileController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IOrderItemRepository orderItemRepository,IOrderRepository orderRepository,IMovieRepository movieRepository,
            ICartRepository cartRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _orderItemRepository = orderItemRepository;
            _orderRepository = orderRepository;
            _movieRepository = movieRepository;
            _cartRepository = cartRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var viewModel = new UserProfileVM
            {
                Id = user.Id,
                UserName = user.UserName!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Email = user.Email!,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                ProfilePicture = user.ProfilePicture // لو عندك خاصية اسمها كده
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var modle = new UserProfileVM
            {
                Id = user.Id,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ProfilePicture = user.ProfilePicture
            };

            return View(modle);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserProfileVM userProfileVM, IFormFile? ImgUrl)
        {
            ModelState.Remove("Email");
            ModelState.Remove("UserName");
            if (!ModelState.IsValid)
                return View(userProfileVM);

            var user = await _userManager.FindByIdAsync(userProfileVM.Id);
            if (user == null) return NotFound();

            // تحديث البيانات
            user.FirstName = userProfileVM.FirstName;
            user.LastName = userProfileVM.LastName;
            user.PhoneNumber = userProfileVM.PhoneNumber;
            user.Address = userProfileVM.Address;

            // تغيير الصورة لو موجودة
            if (ImgUrl != null && ImgUrl.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImgUrl.FileName);
                var filePath = Path.Combine("wwwroot/images/UserProfilePic", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImgUrl.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    var oldImagePath = Path.Combine("wwwroot/images/UserProfilePic", user.ProfilePicture);
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                user.ProfilePicture = fileName;
            }


            // تغيير الباسورد لو مطلوب
            if (!string.IsNullOrWhiteSpace(userProfileVM.OldPassword) && !string.IsNullOrWhiteSpace(userProfileVM.NewPassword))
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, userProfileVM.OldPassword);
                if (!passwordCheck)
                {
                    ModelState.AddModelError("OldPassword", "Old password is incorrect.");
                    return View(userProfileVM);
                }

                var result = await _userManager.ChangePasswordAsync(user, userProfileVM.OldPassword, userProfileVM.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(userProfileVM);
                }
            }

            await _userManager.UpdateAsync(user);
            TempData["success-notification"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> MyTickets()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var orders = await _orderRepository.GetAsync(
                o => o.ApplicationUserId == user.Id && o.OrderStatus == OrderStatus.Completed
            );

            

            var tickets = new List<UserTicketVM>();

            foreach (var order in orders)
            {
                var orderItems = await _orderItemRepository.GetAsync(
                    oi => oi.OrderId == order.Id,
                    includes: [oi => oi.Movie, oi => oi.Movie.cenima, oi => oi.Movie.Category]
                );

                foreach (var item in orderItems)
                {
                    tickets.Add(new UserTicketVM
                    {
                        OrderId = order.Id,
                        OrderDate = order.Date,

                        MovieName = item.Movie.Name,
                        MovieImage = item.Movie.ImgUrl!,
                        MovieStatus = item.Movie.CurrentStatus.ToString(),
                        StartDate = item.Movie.StartDate,
                        EndDate = item.Movie.EndDate,
                        CinemaName = item.Movie.cenima.Name,
                        CategoryName = item.Movie.Category.Name,
                        OrderStatus = item.Order.OrderStatus.ToString(),

                        Quantity = item.Quantity, // أو item.Count لو عندك Count
                        TotalPrice = item.TotalPrice
                    });
                }
            }

            return View(tickets);
        }

    }
}


