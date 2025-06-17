using System.ComponentModel.DataAnnotations.Schema;

namespace Cenima_ETickets.Models
{
    public class ActorMovie
    {       
        public int ActorId { get; set; }              
        public int MovieId { get; set; }

        public Actor actor { get; set; }
        public Movie movie { get; set; }

    }
}
