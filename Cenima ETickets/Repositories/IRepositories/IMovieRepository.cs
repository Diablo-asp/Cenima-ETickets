using Cenima_ETickets.Models;

namespace Cenima_ETickets.Repositories.IRepositories
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Cenima>> GetCenimasAsync();
        Task<List<Actor>> GetActorsAsync();
        Task<List<ActorMovie>> GetActorMoviesAsync();
        Task AddActorsToMovieAsync(int movieId, List<int> actorIds);
        Task<Movie?> GetMovieDetailsAsync(int id);


        // Movie Edit
        Task<EditMovieVM?> GetEditMovieVMAsync(int id);
        Task<bool> UpdateMovieAsync(EditMovieVM vm, IFormFile? ImgUrl);

        // Index Fillter
        Task<IEnumerable<Movie>> GetFilteredAsync(MovieFilterVM filters, int page, int pageSize);
        Task<int> GetTotalCountAsync(MovieFilterVM filters);

        // Delete Action
        Task<bool> DeleteMovieAsync(int id);


    }
}
