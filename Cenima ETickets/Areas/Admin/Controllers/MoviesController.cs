using System.Linq.Expressions;
using System.Threading.Tasks;
using Cinema_ETickets.Data;
using Cinema_ETickets.Models;
using Cinema_ETickets.Utility;
using Cinema_ETickets.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace Cinema_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin},{SD.Employe},{SD.Company}")]

    public class MoviesController : Controller
    {
        private IMovieRepository _movieRepository;
        private ApplicationDbContext _Context = new();
        public MoviesController(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        #region Index
        public async Task<IActionResult> Index(string name, int? categoryId, int? cinemaId, string status = null, int page = 1)
        {
            const int pageSize = 10;

            var filter = new MovieFilterVM
            {
                Name = name,
                CategoryId = categoryId,
                CinemaId = cinemaId,
                Status = status
            };

            var movies = await _movieRepository.GetFilteredAsync(filter, page, pageSize);
            var totalMovies = await _movieRepository.GetTotalCountAsync(filter);

            var viewModel = new MovieIndexViewModelVM
            {
                Movies = movies.ToList(),
                Categories = await _movieRepository.GetCategoriesAsync(),
                Cenimas = await _movieRepository.GetCenimasAsync(),
                Name = name,
                CategoryId = categoryId,
                CinemaId = cinemaId,
                CurrentPage = page,
                TotalNumberOfPages = (int)Math.Ceiling((double)totalMovies / pageSize)
            };

            return View(viewModel);
        }


        #endregion

        #region Create
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

        public async Task<IActionResult> Create()
        {
            var vm = new EditMovieVM
            {
                Categories = await _movieRepository.GetCategoriesAsync(),
                Cenimas = await _movieRepository.GetCenimasAsync(),
                AllActors = await _movieRepository.GetActorsAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Create(EditMovieVM vm, IFormFile ImgUrl)
        {
            ModelState.Remove("Movie.ImgUrl");
            ModelState.Remove("Movie.ActorMovies");
            ModelState.Remove("AllActors");
            ModelState.Remove("Movie.Category");
            ModelState.Remove("Movie.cenima");
            if (!ModelState.IsValid)
            {
                vm.Categories = await _movieRepository.GetCategoriesAsync();
                vm.Cenimas = await _movieRepository.GetCenimasAsync();
                vm.AllActors = await _movieRepository.GetActorsAsync();
                return View(vm);
            }

            // حفظ الصورة
            if (ImgUrl != null && ImgUrl.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImgUrl.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await ImgUrl.CopyToAsync(stream);

                vm.Movie.ImgUrl = fileName;
            }

            // حفظ الفيلم
            await _movieRepository.CreateAsync(vm.Movie);

            // حفظ الممثلين
            if (vm.SelectedActorIds is not null && vm.SelectedActorIds.Any())
            {
                await _movieRepository.AddActorsToMovieAsync(vm.Movie.Id, vm.SelectedActorIds);
            }

            TempData["SuccessMessage"] = "🎉 Movie created successfully!";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieRepository.GetMovieDetailsAsync(id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        #endregion

        #region Edit        
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _movieRepository.GetEditMovieVMAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Edit(int id, EditMovieVM vm, IFormFile? ImgUrl)
        {
            ModelState.Remove("Movie.ImgUrl");
            ModelState.Remove("AllActors");
            ModelState.Remove("Movie.ActorMovies");
            ModelState.Remove("Movie.Category");
            ModelState.Remove("Movie.cenima");
            if (!ModelState.IsValid)
            {
                vm.Categories = await _movieRepository.GetCategoriesAsync();
                vm.Cenimas = await _movieRepository.GetCenimasAsync();
                vm.AllActors = await _movieRepository.GetActorsAsync();
                return View(vm);
            }

            var result = await _movieRepository.UpdateMovieAsync(vm, ImgUrl);
            if (!result) return NotFound();

            TempData["SuccessMessage"] = "🎉 Movie updated successfully!";
            return RedirectToAction("Index");
        }

        #endregion

        #region Delete
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _movieRepository.DeleteMovieAsync(id);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] = "🎉 Movie deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}



