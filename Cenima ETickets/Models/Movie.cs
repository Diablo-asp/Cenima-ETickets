using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;

namespace Cenima_ETickets.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImgUrl { get; set; }
        public string TrairlerUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [NotMapped]
        public MovieStatus CurrentStatus
        {
            get
            {
                var now = DateTime.Now;

                if (now < StartDate)
                    return MovieStatus.upcoming;
                else if (now >= StartDate && now <= EndDate)
                    return MovieStatus.Active;
                else
                    return MovieStatus.Expired;
            }
        }

        public int CenimaId { get; set; }
        public int CategoryId { get; set; }

        public Cenima cenima { get; set; }
        public Category Category { get; set; }
        public ICollection<Actor> actors { get; set; }
        public ICollection<ActorMovie> ActorMovies { get; set; }
    }
}
