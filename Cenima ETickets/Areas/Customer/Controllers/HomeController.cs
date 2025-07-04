using System.Diagnostics;
using Cinema_ETickets.Data;
using Cinema_ETickets.Models;
using Cinema_ETickets.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema_ETickets.Areas.Customers.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext _Context = new();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string? name, int page = 1, int? categoryId = null, int? cinemaId = null)
        {
            IQueryable<Movie> moviesQuery = _Context.movies
                .Include(e => e.cenima)
                .Include(e => e.Category);

            var categories = _Context.categories.ToList();
            var cenimas = _Context.cenimas.ToList();

            const double totalNumberOfMoviesInPages = 9.0;

            #region Filter Movies
            if (!string.IsNullOrEmpty(name))
            {
                moviesQuery = moviesQuery.Where(e => e.Name.Contains(name));
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
            if (totalNumberOfPages < page && totalNumberOfPages > 0)
                return NotFound();

            var movies = moviesQuery
                .Skip((page - 1) * (int)totalNumberOfMoviesInPages)
                .Take((int)totalNumberOfMoviesInPages)
                .ToList();
            #endregion

            #region Slider Movies
            var sliderMovies = _Context.movies
                .OrderByDescending(m => m.StartDate)
                .Take(5)
                .ToList();
            #endregion

            var vm = new MovieIndexViewModelVM
            {
                Movies = movies,
                SliderMovies = sliderMovies,
                Name = name,
                CategoryId = categoryId,
                CinemaId = cinemaId,
                Categories = categories,
                Cenimas = cenimas,
                CurrentPage = page,
                TotalNumberOfPages = (int)totalNumberOfPages
            };

            return View(vm);
        }

        public IActionResult Details(int id)
        {
            var movie = _Context.movies
                .Include(m => m.Category)
                .Include(m => m.cenima)
                .Include(m => m.ActorMovies)
                    .ThenInclude(am => am.actor) 
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            movie.actors = movie.ActorMovies.Select(am => am.actor).ToList();

            return View(movie);
        }

        public IActionResult Actor(int id)
        {
            var actor = _Context.actors.Include(m => m.movies).FirstOrDefault(a => a.Id == id);

            if (actor == null)
                return NotFound();

            return View(actor);
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
