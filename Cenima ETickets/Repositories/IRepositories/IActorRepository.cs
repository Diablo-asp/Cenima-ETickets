namespace Cinema_ETickets.Repositories.IRepositories
{
    public interface IActorRepository : IRepository<Actor>
    {
        Task<Actor?> GetActorWithMoviesAsync(int id);

    }
}
