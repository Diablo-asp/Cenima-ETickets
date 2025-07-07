using System.Text;
using System.Threading.Tasks;
using Cinema_ETickets.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Common;

namespace Cinema_ETickets.Areas.Identity.Controllers
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
                    "Account", new { userId = user.Id, token, area = "Identity" }, Request.Scheme);

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


        // SignOut Action
        public new async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            TempData["success-notification"] = "SignOut Successfully";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
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
                    TempData["error-notification"] = $"{string.Join(",", result.Errors)}";
                }

                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            return NotFound();
        }

        
        // Resend Condirm Email
        public IActionResult ResendEmailConfirmation()
        {            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
            {
                return View(resendEmailConfirmationVM);
            }

            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationVM.EmailOrUserName)
                       ?? await _userManager.FindByNameAsync(resendEmailConfirmationVM.EmailOrUserName);

            if (user is null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(resendEmailConfirmationVM);
            }

            // Check if already confirmed
            if (user.EmailConfirmed)
            {
                TempData["error-notification"] = "Your email is already confirmed.";
                return RedirectToAction("Login");
            }

            // Check resend delay (15 minutes)
            if (user.EmailConfirmationSentAt.HasValue &&
                user.EmailConfirmationSentAt.Value.AddMinutes(15) > DateTime.UtcNow)
            {
                var remainingMinutes = (user.EmailConfirmationSentAt.Value.AddMinutes(15) - DateTime.UtcNow).Minutes;
                TempData["error-notification"] = $"Please wait {remainingMinutes} minute(s) before requesting a new confirmation email.";
                return View(resendEmailConfirmationVM);
            }

            // Send new confirmation email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token, area = "Identity" }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!, "Confirm Your Account Again!",
                $"<h1>Click <a href='{link}'>here</a> to confirm your account.</h1>");

            // Update sent time
            user.EmailConfirmationSentAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            TempData["success-notification"] = "Confirmation email sent successfully.";
            return RedirectToAction("Login");
        }

        // Forget Password Action
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(forgetPasswordVM);
            }

            var user = await _userManager.FindByEmailAsync(forgetPasswordVM.EmailOrUserName);

            if (user is null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(forgetPasswordVM);
            }

            if (!user.EmailConfirmed)
            {
                TempData["error-notification"] = "Your email needs to be confirmed first.";
                return View(forgetPasswordVM); 
            }

            // Check resend delay (15 minutes)

            if (user.EmailConfirmationSentAt.HasValue &&
                user.EmailConfirmationSentAt.Value.AddMinutes(10) > DateTime.UtcNow)
            {
                var remainingMinutes = (user.EmailConfirmationSentAt.Value.AddMinutes(10) - DateTime.UtcNow).Minutes;
                TempData["error-notification"] = $"Please wait {remainingMinutes} minute(s) before requesting a new confirmation email.";
                return View(forgetPasswordVM);
            }

            // Generate token and encode it
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action(nameof(ChangePassword), "Account",
                new { userId = user.Id, token, area = "Identity" }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!, "Change Your Password!",
                $"<h1>Click <a href='{link}'>here</a> to Reset your Password.</h1>");

            // Save time of last request (optional) 
            user.EmailConfirmationSentAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            TempData["success-notification"] = "Reset link sent to your email successfully.";

            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }


        // Change Password Action
        public async Task<IActionResult> ChangePassword(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId); 

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["error-notification"] = "Invalid password reset request.";
                return RedirectToAction("Login");
            }


            if (user is not null)
            {
                var changePasswordVM = new ChangePasswordVM
                {
                    UserId = userId,
                    Token = token
                };

                return View(changePasswordVM);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(changePasswordVM);
            }

            var user = await _userManager.FindByIdAsync(changePasswordVM.UserId);
            
            if (user is not null)
            {
                var result = await _userManager.ResetPasswordAsync(user,token: changePasswordVM.Token, changePasswordVM.Password);

                if (result.Succeeded)
                {
                    // Save the DateTime Password Changed
                    user.PasswordLastChangedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    TempData["success-notification"] = "Password changed successfully.";
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(changePasswordVM);
            }
            TempData["error-notification"] = "User not found.";
            return RedirectToAction("Login");
        }


    }
}
