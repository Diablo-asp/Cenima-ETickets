using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Cenima_ETickets.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cenima_ETickets.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppcationDbContext _Context = new();
        [Area("Admin")]
        public IActionResult Index()
        {
            var totalMovies = _Context.movies.Count();
            var totalCinemas = _Context.cenimas.Count();
            var totalActors = _Context.actors.Count();
            var actors = _Context.actors.Include(e => e.movies);


            var availableMovies = _Context.movies.AsEnumerable().Count(e => e.CurrentStatus == MovieStatus.Active);
            var upComingMovies = _Context.movies.AsEnumerable().Count(e => e.CurrentStatus == MovieStatus.upcoming);
            var expiredMovies = _Context.movies.AsEnumerable().Count(e => e.CurrentStatus == MovieStatus.Expired);

            var vm = new DashBoardAdminVM
            {
                TotalMovies = totalMovies,
                TotalCinemas = totalCinemas,
                TotalActors = totalActors,
                AvailableMovies = availableMovies,
                UpcomingMovies = upComingMovies,
                ExpiredMovies = expiredMovies,
                Actors = actors.ToList()
            };
            return View(vm);
        }
    }
}
