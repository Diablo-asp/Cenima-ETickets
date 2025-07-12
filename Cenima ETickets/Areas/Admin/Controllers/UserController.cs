using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Cinema_ETickets.Utility;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;

namespace Cinema_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin},{SD.Employe},{SD.Company}")]
    public class UserController : Controller
    {

        //private ApplicationDbContext _context;// = new();
        private UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public async Task<IActionResult> Index()
        {
            var result = _userManager.Users.ToList();
            Dictionary<ApplicationUser, string> keyValuePairs = new();
            foreach (var item in result)
            {
                keyValuePairs.Add(item, String.Join(",", await _userManager.GetRolesAsync(item)));
            }
            return View(keyValuePairs.ToDictionary());
        }

        #region Create
        public IActionResult Create()
        {
            var user = new RegisterVM
            {
                Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList()
            };

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterVM registerVM, List<string> Roles)
        {
            ModelState.Remove("Id");
            if (!ModelState.IsValid)
            {
                registerVM.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();
                return View(registerVM);
            }

            var user = new ApplicationUser
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Address = registerVM.Address
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (result.Succeeded)
            {
                if (Roles.Any())
                {
                    await _userManager.AddToRolesAsync(user, Roles);
                }

                TempData["success-notification"] = "User created successfully.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            registerVM.Roles = _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            }).ToList();

            return View(registerVM);
        }
        #endregion

        #region Edit
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var registerVM = new RegisterVM
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Address = user.Address,
                Roles = allRoles.Select(role => new SelectListItem
                {
                    Text = role,
                    Value = role,
                    Selected = userRoles.Contains(role)
                }).ToList()
            };

            return View(registerVM);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(RegisterVM registerVM, List<string> Roles)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            var user = await _userManager.FindByIdAsync(registerVM.Id);

            if (user is not null)
            {
                user.FirstName = registerVM.FirstName;
                user.LastName = registerVM.LastName;
                user.UserName = registerVM.UserName;
                user.Email = registerVM.Email;
                user.Address = registerVM.Address;
                await _userManager.UpdateAsync(user);

                var userRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, userRoles);

                await _userManager.AddToRolesAsync(user, Roles);

                TempData["success-notification"] = "Edit User Success";
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        #endregion

        #region Delete
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userManager.DeleteAsync(user);
            TempData["success-notification"] = "user deleted successfully!";
            return RedirectToAction("Index");
        }

        #endregion
    }
}
