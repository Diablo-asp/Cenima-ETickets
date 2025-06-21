using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Cenima_ETickets.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cenima_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        private AppcationDbContext _Context = new();
        #region Index
        public IActionResult Index(string name, int? categoryId, int? cinemaId, string status = null, int page = 1)
        {
            const int pageSize = 10;

            var moviesQuery = _Context.movies
                .Include(m => m.Category)
                .Include(m => m.cenima)
                .AsQueryable();

            #region Filtering

            if (!string.IsNullOrEmpty(name))
            {
                moviesQuery = moviesQuery.Where(m => m.Name.Contains(name));
            }

            if (categoryId.HasValue)
            {
                moviesQuery = moviesQuery.Where(m => m.CategoryId == categoryId.Value);
            }

            if (cinemaId.HasValue)
            {
                moviesQuery = moviesQuery.Where(m => m.CenimaId == cinemaId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                moviesQuery = status switch
                {
                    "Active" => moviesQuery.AsEnumerable().Where(m => m.CurrentStatus == MovieStatus.Active).AsQueryable(),
                    "Upcoming" => moviesQuery.AsEnumerable().Where(m => m.CurrentStatus == MovieStatus.upcoming).AsQueryable(),
                    "Expired" => moviesQuery.AsEnumerable().Where(m => m.CurrentStatus == MovieStatus.Expired).AsQueryable(),
                    _ => moviesQuery
                };
            }

            #endregion

            #region Pagination

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
        #endregion

        #region Create
        public IActionResult Create()
        {
            var vm = new EditMovieVM
            {
                Cenimas = _Context.cenimas.ToList(),
                Categories = _Context.categories.ToList(),
                AllActors = _Context.actors.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EditMovieVM vm)
        {

            ModelState.Remove("Movie.CurrentStatus");

            _Context.movies.Add(vm.Movie);
            _Context.SaveChanges();

            // العلاقة Many-to-Many
            if (vm.SelectedActorIds.Any())
            {
                foreach (var actorId in vm.SelectedActorIds)
                {
                    _Context.ActorMovies.Add(new ActorMovie
                    {
                        ActorId = actorId,
                        MovieId = vm.Movie.Id
                    });
                }
                _Context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Details
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
        #endregion

        #region Edit
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var movie = _Context.movies
                .Include(m => m.Category)
                .Include(m => m.cenima)
                .Include(m => m.ActorMovies)
                .FirstOrDefault(m => m.Id == Id);

            if (movie == null) return NotFound();

            var vm = new EditMovieVM
            {
                Movie = movie,
                Cenimas = _Context.cenimas.ToList(),
                Categories = _Context.categories.ToList(),
                AllActors = _Context.actors.ToList(),
                SelectedActorIds = movie.ActorMovies.Select(am => am.ActorId).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(EditMovieVM vm)
        {
            var movieInDb = _Context.movies
                .Include(m => m.ActorMovies)
                .FirstOrDefault(m => m.Id == vm.Movie.Id);

            if (movieInDb == null) return NotFound();

            // تحديث بيانات الفيلم الأساسية
            movieInDb.Name = vm.Movie.Name;
            movieInDb.Description = vm.Movie.Description;
            movieInDb.Price = vm.Movie.Price;
            movieInDb.ImgUrl = vm.Movie.ImgUrl;
            movieInDb.TrairlerUrl = vm.Movie.TrairlerUrl;
            movieInDb.StartDate = vm.Movie.StartDate;
            movieInDb.EndDate = vm.Movie.EndDate;
            movieInDb.CenimaId = vm.Movie.CenimaId;
            movieInDb.CategoryId = vm.Movie.CategoryId;

            // تحديث الممثلين
            _Context.ActorMovies.RemoveRange(movieInDb.ActorMovies);
            foreach (var actorId in vm.SelectedActorIds)
            {
                _Context.ActorMovies.Add(new ActorMovie
                {
                    MovieId = movieInDb.Id,
                    ActorId = actorId
                });
            }

            _Context.SaveChanges();
            return RedirectToAction("Index");
        }

        #endregion

        #region Delete
        public IActionResult Delete(int id)
        {
            var category = _Context.movies.Find(id);

            if (category is not null)
            {
                _Context.movies.Remove(category);
                _Context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }
        #endregion
    }
}



