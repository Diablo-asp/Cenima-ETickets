using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;

namespace Cenima_ETickets.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public double Price { get; set; }
        [Required(ErrorMessage ="You need to Upload Photo") ]        
        public string ImgUrl { get; set; }
        public string? TrairlerUrl { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
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
        
        public Cenima? cenima { get; set; }
        public Category? Category { get; set; }
        public ICollection<Actor>? actors { get; set; }
        public ICollection<ActorMovie> ActorMovies { get; set; }
    }
}
