using System.ComponentModel.DataAnnotations;

namespace Cinema_ETickets.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        public string Name { get; set; } = string.Empty;
        public string? CategoryUrl { get; set; }


        public ICollection<Movie> movies { get; set; } = new List<Movie>();
    }
}
