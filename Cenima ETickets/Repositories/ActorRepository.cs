using Microsoft.EntityFrameworkCore;

public class ActorRepository : Repository<Actor>, IActorRepository
{
    private readonly ApplicationDbContext _context;

    public ActorRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Actor?> GetActorWithMoviesAsync(int id)
    {
        return await _context.actors
            .Include(a => a.ActorMovies)
                .ThenInclude(am => am.movie)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}
