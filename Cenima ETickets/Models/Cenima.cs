using System.ComponentModel.DataAnnotations;

namespace Cinema_ETickets.Models
{
    public class Cenima
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } 
        public string? Description { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        public string CenimaLogo { get; set; }


        public ICollection<Movie> movies { get; set; }
    }
}
