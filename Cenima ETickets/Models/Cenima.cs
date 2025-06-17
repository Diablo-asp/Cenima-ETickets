namespace Cenima_ETickets.Models
{
    public class Cenima
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CenimaLogo { get; set; }
        public string Address { get; set; }


        public ICollection<Movie> movies { get; set; }
    }
}
