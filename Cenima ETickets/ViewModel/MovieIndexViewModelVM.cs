using Cinema_ETickets.Models;

namespace Cinema_ETickets.ViewModel
{
    public class MovieIndexViewModelVM
    {
        public List<Movie> Movies { get; set; } = null!; // Initialize to avoid null reference exceptions
        public List<Movie>? SliderMovies { get; set; }
        public Movie Movie { get; set; } = null!;

        public string Name { get; set; } = null!;
        public int? CategoryId { get; set; }
        public int? CinemaId { get; set; }

        public List<Category> Categories { get; set; }
        public List<Cenima> Cenimas { get; set; }

        public int CurrentPage { get; set; }
        public int TotalNumberOfPages { get; set; }
    }


}
