namespace Cenima_ETickets.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? CategoryUrl { get; set; }


        public ICollection<Movie> movies { get; set; }
    }
}
