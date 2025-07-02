using Microsoft.EntityFrameworkCore;

public class CinemaRepository : Repository<Cenima>, ICinemaRepository
{
    private readonly ApplicationDbContext _context;

    public CinemaRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Cenima?> GetCinemaWithMoviesAsync(int id)
    {
        return await _context.cenimas
            .Include(c => c.movies)
            .ThenInclude(m => m.Category)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
