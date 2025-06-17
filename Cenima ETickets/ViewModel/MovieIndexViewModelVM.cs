using Cenima_ETickets.Models;

namespace Cenima_ETickets.ViewModel
{
    public class MovieIndexViewModelVM
    {
        public List<Movie> Movies { get; set; }
        public List<Movie> SliderMovies { get; set; }

        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public int? CinemaId { get; set; }

        public List<Category> Categories { get; set; }
        public List<Cenima> Cenimas { get; set; }

        public int CurrentPage { get; set; }
        public int TotalNumberOfPages { get; set; }
    }


}
