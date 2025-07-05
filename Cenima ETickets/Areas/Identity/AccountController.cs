using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace Cinema_ETickets.Areas.Identity
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender ;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountController(UserManager<ApplicationUser> userManager,IEmailSender emailSender, SignInManager<ApplicationUser> signInManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if(!ModelState.IsValid)
            {
                return View(registerVM);
            }


            ApplicationUser user = new()
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Address = registerVM.Adrress
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);


            if (result.Succeeded)
            {
                // login the user
                TempData["success-notification"] = "User registered successfully";

                //send Confirmation Email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action(nameof(ConfirmEmail),
                    "Account", new { userId = user.Id, token = token, area = "Identity" }, Request.Scheme);

                await _emailSender.SendEmailAsync(user!.Email ?? "",
                    "Confirm Your Account", $"<h1>Confirm Your Account By Clicking <a href='{link}'>here</a></h1>");

                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }

            return View(registerVM);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            var user = await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail);

            if (user is null)
            {
                user = await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);
            }

            if (user is not null)
            {
                var result = await _userManager.CheckPasswordAsync(user, loginVM.Password);

                if (result)
                {
                    if (!user.EmailConfirmed)
                    {
                        TempData["error-notification"] = "Confirm Your Email";
                        return View(loginVM);
                    }

                    if (!user.LockoutEnabled)
                    {
                        TempData["error-notification"] = $"You have block till {user.LockoutEnd}";
                        return View(loginVM);
                    }


                    await _signInManager.SignInAsync(user, loginVM.RememberMe);
                    TempData["success-notification"] = "Login Successfully";
                    return RedirectToAction("Index", "Home", new { area = "Customer" });

                }
            }

            ModelState.AddModelError("UserNameOrEmail", "Invalid UserName Or Email");
            ModelState.AddModelError("Password", "Invalid Password");
            return View(loginVM);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is not null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    TempData["success-notification"] = "Confirm Email Successfully";
                }
                else
                {
                    TempData["error-notification"] = $"{String.Join(",", result.Errors)}";
                }

                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            return NotFound();
        }
    }
}
