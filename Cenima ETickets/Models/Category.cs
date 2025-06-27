using System.ComponentModel.DataAnnotations;

namespace Cenima_ETickets.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        public string Name { get; set; }
        public string? CategoryUrl { get; set; }


        public ICollection<Movie> movies { get; set; }
    }
}
