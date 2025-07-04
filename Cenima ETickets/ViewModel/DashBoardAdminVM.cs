using Cinema_ETickets.Models;

namespace Cinema_ETickets.ViewModel
{
    public class DashBoardAdminVM
    {
        public int TotalMovies { get; set; }
        public int TotalCinemas { get; set; }
        public int TotalActors { get; set; }
        public int AvailableMovies { get; set; }
        public int UpcomingMovies { get; set; }
        public int ExpiredMovies { get; set; }
        public IEnumerable<Actor> Actors { get; set; } = null!;

    }
}
