using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cenima_ETickets.Models
{
    public class ActorMovie
    {
        [Required]
        public int ActorId { get; set; }
        [Required]
        public int MovieId { get; set; }

        public Actor actor { get; set; }
        public Movie movie { get; set; }

    }
}
