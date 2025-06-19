using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Cenima_ETickets.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cenima_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        private AppcationDbContext _Context = new();
        public IActionResult Index(string name, int? categoryId, int? cinemaId, int page = 1)
        {
            const int pageSize = 10;

            var moviesQuery = _Context.movies
                .Include(m => m.Category)
                .Include(m => m.cenima)
                .AsQueryable();
            #region Filltering
            // فلترة حسب الاسم
            if (!string.IsNullOrEmpty(name))
            {
                moviesQuery = moviesQuery.Where(m => m.Name.Contains(name));
            }

            // فلترة حسب الكاتجوري
            if (categoryId.HasValue)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId.Value);
            }

            // فلترة حسب السينما
            if (cinemaId.HasValue)
            {
                moviesQuery = moviesQuery.Where(m => m.CenimaId == cinemaId.Value);
            }
            #endregion

            #region Pagnation
            int totalMovies = moviesQuery.Count();
            int totalPages = (int)Math.Ceiling((double)totalMovies / pageSize);

            var movies = moviesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            #endregion

            var viewModel = new MovieIndexViewModelVM
            {
                Movies = movies,
                Categories = _Context.categories.ToList(),
                Cenimas = _Context.cenimas.ToList(),
                Name = name,
                CategoryId = categoryId,
                CinemaId = cinemaId,
                CurrentPage = page,
                TotalNumberOfPages = totalPages
            };

            return View(viewModel);
        }


        public IActionResult Create()
        {
            var viewModel = new MovieIndexViewModelVM
            {
                Movie = new Movie(),
                Cenimas = _Context.cenimas.ToList(),
                Categories = _Context.categories.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MovieIndexViewModelVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Cenimas = _Context.cenimas.ToList();
                vm.Categories = _Context.categories.ToList();
                return View(vm);
            }

            _Context.movies.Add(vm.Movie);
            _Context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


    }
}
