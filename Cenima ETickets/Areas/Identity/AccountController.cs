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
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Register Action
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(registerVM.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
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

                user.EmailConfirmationSentAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }

            return View(registerVM);
        }


        // Login Action
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail)
                       ?? await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);

            if (user != null)
            {
                var passwordCorrect = await _userManager.CheckPasswordAsync(user, loginVM.Password);

                if (passwordCorrect)
                {
                    if (!user.EmailConfirmed)
                    {
                        // Check if 24 hours passed since confirmation email was sent
                        if (user.EmailConfirmationSentAt.HasValue &&
                            user.EmailConfirmationSentAt.Value.AddHours(24) <= DateTime.UtcNow)
                        {
                            TempData["error-notification"] = "Your confirmation link has expired. Please confirm again.";
                            return RedirectToAction("ResendEmailConfirmation", "Account");
                        }

                        TempData["error-notification"] = "Please confirm your email.";
                        return View(loginVM);
                    }

                    if (!user.LockoutEnabled)
                    {
                        TempData["error-notification"] = $"You have been blocked until {user.LockoutEnd}";
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


        // ConfirmEmail Action
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

        
        // Resend Condirm Email
        public IActionResult ResendConfirmEmail()
        {            
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendConfirmEmailVM resendConfirmEmailVM)
        {
            if (!ModelState.IsValid)
            {
                return View(resendConfirmEmailVM);
            }

            var user = await _userManager.FindByEmailAsync(resendConfirmEmailVM.EmailOrUserName)
                       ?? await _userManager.FindByNameAsync(resendConfirmEmailVM.EmailOrUserName);

            if (user is null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(resendConfirmEmailVM);
            }

            // Check if already confirmed
            if (user.EmailConfirmed)
            {
                TempData["info-notification"] = "Your email is already confirmed.";
                return RedirectToAction("Login");
            }

            // Check resend delay (15 minutes)
            if (user.EmailConfirmationSentAt.HasValue &&
                user.EmailConfirmationSentAt.Value.AddMinutes(15) > DateTime.UtcNow)
            {
                var remainingMinutes = (user.EmailConfirmationSentAt.Value.AddMinutes(15) - DateTime.UtcNow).Minutes;
                TempData["warning-notification"] = $"Please wait {remainingMinutes} minute(s) before requesting a new confirmation email.";
                return View(resendConfirmEmailVM);
            }

            // Send new confirmation email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token = token, area = "Identity" }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!, "Confirm Your Account Again!",
                $"<h1>Click <a href='{link}'>here</a> to confirm your account.</h1>");

            // Update sent time
            user.EmailConfirmationSentAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            TempData["success-notification"] = "Confirmation email sent successfully.";
            return RedirectToAction("Login");
        }



    }
}
