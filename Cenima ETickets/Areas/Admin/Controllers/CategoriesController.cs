using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cenima_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private AppcationDbContext _context = new();
        public IActionResult Index()
        {
            var categories = _context.categories;
            return View(categories.ToList());
        }

        #region Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string Name, IFormFile CategoryImage)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(CategoryImage.FileName);
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categorys", fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await CategoryImage.CopyToAsync(stream);
            }

            var newCategory = new Category
            {
                Name = Name,
                CategoryUrl = fileName
            };

            _context.categories.Add(newCategory);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        #endregion

        #region Edit
        public IActionResult Edit(int id)
        {
            var category = _context.categories.FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string Name, IFormFile? NewImage)
        {
            var category = _context.categories.FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();

            category.Name = Name;

            if (NewImage is not null && NewImage.Length > 0)
            {
                // حذف الصورة القديمة
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categorys", category.CategoryUrl);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                // حفظ الصورة الجديدة
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(NewImage.FileName);
                var newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categorys", fileName);

                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await NewImage.CopyToAsync(stream);
                }

                category.CategoryUrl = fileName;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        #endregion

        #region Delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var category = _context.categories.FirstOrDefault(c => c.Id == id);
            if (category is null) return NotFound();

            // مسار الصورة
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categorys", category.CategoryUrl);

            // حذف الصورة من السيرفر لو موجودة
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            // حذف التصنيف من قاعدة البيانات
            _context.categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        #endregion

        #region Movie By Category
        public IActionResult MoviesByCategory(int id)
        {
            var category = _context.categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
                return NotFound();

            var movies = _context.movies
                .Include(m => m.cenima)
                .Include(m => m.Category)
                .Where(m => m.CategoryId == id)
                .ToList();

            ViewBag.CategoryName = category.Name;

            return View(movies);
        }
        #endregion
    }
}
