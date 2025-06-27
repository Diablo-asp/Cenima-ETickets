using System.Threading.Tasks;
using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Cenima_ETickets.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

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
        public async Task<IActionResult> Create(EditMovieVM vm, IFormFile ImgUrl)
        {
            //ModelState.Remove("ImgUrl");
            ModelState.Remove("Movie.ImgUrl");
            ModelState.Remove("AllActors");
            ModelState.Remove("Movie.ActorMovies");
            if (ModelState.IsValid)
            {

                if (ImgUrl is not null && ImgUrl.Length > 0)
                {
                    var FileName = Guid.NewGuid().ToString() + Path.GetExtension(ImgUrl.FileName);
                    var FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movies", FileName);

                    using (var stream = System.IO.File.Create(FilePath))
                    {
                        await ImgUrl.CopyToAsync(stream);
                    }
                    vm.Movie.ImgUrl = FileName;
                }


                _Context.movies.Add(vm.Movie);
                _Context.SaveChanges();

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
                TempData["SuccessMessage"] = "🎉 Movie created successfully!";

                return RedirectToAction(nameof(Index));
            }
             vm = new EditMovieVM
            {
                Cenimas = _Context.cenimas.ToList(),
                Categories = _Context.categories.ToList(),
                AllActors = _Context.actors.ToList()
            };
            return View(vm);
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
        public async Task<IActionResult> Edit(int Id,EditMovieVM vm, IFormFile? ImgUrl)
        {
            ModelState.Remove("Movie.ImgUrl");
            ModelState.Remove("AllActors");
            ModelState.Remove("Movie.ActorMovies");
            //ModelState.Remove("ImgUrl");
            if (ModelState.IsValid)
            {
                var ImgInDb = _Context.movies.Find(vm.Movie.Id);

                if (ImgInDb is not null && ImgUrl is not null && ImgUrl.Length > 0)
                {
                    var FileName = Guid.NewGuid().ToString() + Path.GetExtension(ImgUrl.FileName);
                    var FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movies", FileName);

                    using (var stream = System.IO.File.Create(FilePath))
                    {
                        await ImgUrl.CopyToAsync(stream);
                    }
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movies", ImgInDb.ImgUrl);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    vm.Movie.ImgUrl = FileName;
                }


                var movieInDb = _Context.movies
                    .Include(m => m.ActorMovies)
                    .FirstOrDefault(m => m.Id == vm.Movie.Id);

                if (movieInDb == null) return NotFound();

                // تحديث بيانات الفيلم الأساسية
                movieInDb.Name = vm.Movie.Name;
                movieInDb.Description = vm.Movie.Description;
                movieInDb.Price = vm.Movie.Price;
                if (ImgUrl != null && ImgUrl.Length > 0)
                {
                    movieInDb.ImgUrl = vm.Movie.ImgUrl;
                }
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
                TempData["SuccessMessage"] = "🎉 Movie Edit successfully!";

                return RedirectToAction("Index");
            }

            var movie = _Context.movies
                .Include(m => m.Category)
                .Include(m => m.cenima)
                .Include(m => m.ActorMovies)
                .FirstOrDefault(m => m.Id == Id);

            if (movie == null) return NotFound();

             vm = new EditMovieVM
            {
                Movie = movie,
                Cenimas = _Context.cenimas.ToList(),
                Categories = _Context.categories.ToList(),
                AllActors = _Context.actors.ToList(),
                SelectedActorIds = movie.ActorMovies.Select(am => am.ActorId).ToList()                
            };
            return View(vm);
        }

        #endregion

        #region Delete
        public IActionResult Delete(int id)
        {
            var movie = _Context.movies.FirstOrDefault(m => m.Id == id);

            if (movie is not null)
            {
                if (!string.IsNullOrEmpty(movie.ImgUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies", movie.ImgUrl);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _Context.movies.Remove(movie);
                _Context.SaveChanges();
                TempData["SuccessMessage"] = "🎉 Movie Delete successfully!";

                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }

        #endregion
    }
}



