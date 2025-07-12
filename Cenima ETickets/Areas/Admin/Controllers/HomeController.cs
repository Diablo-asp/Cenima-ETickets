using Cinema_ETickets.Data;
using Cinema_ETickets.Models;
using Cinema_ETickets.Utility;
using Cinema_ETickets.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin},{SD.Employe},{SD.Company}")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _Context = new();
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
