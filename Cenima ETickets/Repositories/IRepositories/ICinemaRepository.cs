using System.Linq.Expressions;

public interface ICinemaRepository : IRepository<Cenima>
{
    Task<Cenima?> GetCinemaWithMoviesAsync(int id);
}
