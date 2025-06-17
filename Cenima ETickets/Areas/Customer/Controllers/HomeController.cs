using System.Diagnostics;
using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Cenima_ETickets.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cenima_ETickets.Areas.Customers.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private AppcationDbContext _Context = new();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(Movie movie, int page = 1, int? categoryId = null, int? cinemaId = null)
        {
            IQueryable<Movie> moviesQuery = _Context.movies
                .Include(e => e.cenima)
                .Include(e => e.Category);

            var categories = _Context.categories.ToList();
            var cenimas = _Context.cenimas.ToList();

            const double totalNumberOfMoviesInPages = 9.0;

            #region Filter Movies
            if (!string.IsNullOrEmpty(movie.Name))
            {
                moviesQuery = moviesQuery.Where(e => e.Name.Contains(movie.Name));
            }

            if (categoryId.HasValue)
            {
                moviesQuery = moviesQuery.Where(e => e.CategoryId == categoryId.Value);
            }

            if (cinemaId.HasValue)
            {
                moviesQuery = moviesQuery.Where(e => e.CenimaId == cinemaId.Value);
            }
            #endregion

            #region Pagination
            var totalNumberOfPages = Math.Ceiling(moviesQuery.Count() / totalNumberOfMoviesInPages);
            if (totalNumberOfPages < page)
                return NotFound();

            var movies = moviesQuery
                .Skip((page - 1) * (int)totalNumberOfMoviesInPages)
                .Take((int)totalNumberOfMoviesInPages)
                .ToList();
            #endregion

            #region ???????? - ??? 5 ????? (??? ?????? ?????)
            var sliderMovies = _Context.movies
                .OrderByDescending(m => m.StartDate)
                .Take(5)
                .ToList();
            #endregion

            var vm = new MovieIndexViewModelVM
            {
                Movies = movies,
                SliderMovies = sliderMovies,
                Name = movie.Name,
                CategoryId = categoryId,
                CinemaId = cinemaId,
                Categories = categories,
                Cenimas = cenimas,
                CurrentPage = page,
                TotalNumberOfPages = (int)totalNumberOfPages
            };

            return View(vm);
        }





        public IActionResult Details(int Id)
        {
            var actorMovie = _Context.ActorMovies
                    .Include(m => m.movie.cenima)
                    .Include(m => m.movie.Category)
                    .Include(a => a.actor)
                    .Include(b => b.movie)
                    .FirstOrDefault(m => m.movie.Id == Id);

            if (actorMovie == null)
                return NotFound();

            return View(actorMovie);
        }

        public IActionResult Categorys(int Id)
        {
            var category = _Context.categories;

            return View(category.ToList());
        }

        public IActionResult Cinemas(int Id)
        {
            var cinemas = _Context.cenimas.ToList();
            return View(cinemas);
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
