using System.Linq.Expressions;
using System.Threading.Tasks;
using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Cenima_ETickets.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cenima_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        IdentityUser _userManager = new IdentityUser();

        private ApplicationDbContext _context = new();
        private ICategoryRepository _categoryRepository;// = new();
        private IMovieRepository _movieRepository;// = new();



        public CategoriesController(ICategoryRepository categoryRepository, IMovieRepository movieRepository)
        {
            _categoryRepository = categoryRepository;
            _movieRepository = movieRepository;
        }


        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAsync();
            return View(categories);
        }

        #region Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string Name, IFormFile CategoryUrl)
        {
            if (ModelState.IsValid)
            {

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(CategoryUrl.FileName);
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categorys", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await CategoryUrl.CopyToAsync(stream);
                }

                var newCategory = new Category
                {
                    Name = Name,
                    CategoryUrl = fileName
                };

               await _categoryRepository.CreateAsync(newCategory);
                TempData["SuccessMessage"] = "🎉 Category created successfully!";

                return RedirectToAction("Index");
            }
            return View();
        }
        #endregion

        #region Edit
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string Name, IFormFile? NewImage)
        {
            if (ModelState.IsValid)
            {
                var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
                if (category == null) return NotFound();

                category.Name = Name;

                if (NewImage is not null && NewImage.Length > 0)
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categorys", category.CategoryUrl ?? "");
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(NewImage.FileName);
                    var newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categorys", fileName);

                    using (var stream = new FileStream(newPath, FileMode.Create))
                    {
                        await NewImage.CopyToAsync(stream);
                    }

                    category.CategoryUrl = fileName;
                }

                await _categoryRepository.UpdateAsync(category);
                TempData["SuccessMessage"] = "🎉 Category edited successfully!";
                return RedirectToAction("Index");
            }

            var fallbackCategory = await _categoryRepository.GetOneAsync(e => e.Id == id);
            return fallbackCategory is null ? NotFound() : View(fallbackCategory);
        }

        #endregion

        #region Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id);
            if (category == null) return NotFound();

            if (!string.IsNullOrEmpty(category.CategoryUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categorys", category.CategoryUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _categoryRepository.DeleteAsync(category);
            TempData["SuccessMessage"] = "🎉 Category deleted successfully!";
            return RedirectToAction("Index");
        }

        #endregion

        #region Movie By Category
        public async Task<IActionResult> MoviesByCategory(int id)
        {
            // هات الكاتجوري
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null)
                return NotFound();

            // هات الأفلام المرتبطة بيه مع السينما والتصنيف
            var includes = new Expression<Func<Movie, object>>[]
            {
            m => m.cenima!,
            m => m.Category!
            };

            var movies = await _movieRepository.GetAsync(m => m.CategoryId == id, includes);

            ViewBag.CategoryName = category.Name;
            return View(movies);
        }
        #endregion
    }
}
