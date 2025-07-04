using System.ComponentModel.DataAnnotations;

namespace Cinema_ETickets.Models
{
    public class Actor
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Bio { get; set; }
        [Required]
        public string ProfilePic { get; set; }
        public string? News { get; set; }


        public ICollection<Movie> movies { get; set; }
        public ICollection<ActorMovie> ActorMovies { get; set; }
    }
}
