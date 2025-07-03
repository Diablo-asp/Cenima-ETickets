using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Cenima_ETickets.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Cenima_ETickets.Repositories
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Movie> _db;

        public MovieRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _db = _context.Set<Movie>();
        }
        

        #region Fillter with Pagenation
        public async Task<IEnumerable<Movie>> GetFilteredAsync(MovieFilterVM filters, int page, int pageSize)
        {
            var query = _db
                .Include(m => m.Category!)
                .Include(m => m.cenima!)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filters.Name))
                query = query.Where(m => m.Name.Contains(filters.Name));

            if (filters.CategoryId.HasValue)
                query = query.Where(m => m.CategoryId == filters.CategoryId);

            if (filters.CinemaId.HasValue)
                query = query.Where(m => m.CenimaId == filters.CinemaId);

            if (!string.IsNullOrEmpty(filters.Status))
            {
                query = filters.Status switch
                {
                    "Active" => query.Where(m => m.CurrentStatus == MovieStatus.Active),
                    "Upcoming" => query.Where(m => m.CurrentStatus == MovieStatus.upcoming),
                    "Expired" => query.Where(m => m.CurrentStatus == MovieStatus.Expired),
                    _ => query
                };
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(MovieFilterVM filters)
        {
            var query = _db.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Name))
                query = query.Where(m => m.Name.Contains(filters.Name));

            if (filters.CategoryId.HasValue)
                query = query.Where(m => m.CategoryId == filters.CategoryId);

            if (filters.CinemaId.HasValue)
                query = query.Where(m => m.CenimaId == filters.CinemaId);

            if (!string.IsNullOrEmpty(filters.Status))
            {
                query = filters.Status switch
                {
                    "Active" => query.Where(m => m.CurrentStatus == MovieStatus.Active),
                    "Upcoming" => query.Where(m => m.CurrentStatus == MovieStatus.upcoming),
                    "Expired" => query.Where(m => m.CurrentStatus == MovieStatus.Expired),
                    _ => query
                };
            }

            return await query.CountAsync();
        }

        #endregion

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.categories.ToListAsync();
        }

        public async Task<List<Cenima>> GetCenimasAsync()
        {
            return await _context.cenimas.ToListAsync();
        }

        public async Task<List<Actor>> GetActorsAsync()
        {
            return await _context.actors.ToListAsync();
        }

        public async Task<List<ActorMovie>> GetActorMoviesAsync()
        {
            return await _context.ActorMovies.ToListAsync();
        }

        #region Add Actors to the Movie(Action(Create))
        public async Task AddActorsToMovieAsync(int movieId, List<int> actorIds)
        {
            var actorMovies = actorIds.Select(actorId => new ActorMovie
            {
                MovieId = movieId,
                ActorId = actorId
            });

            await _context.ActorMovies.AddRangeAsync(actorMovies);
            await _context.SaveChangesAsync();
        }
        #endregion

        #region Movie Details
        public async Task<Movie?> GetMovieDetailsAsync(int id)
        {
            var movie = await _context.movies
                .Include(m => m.Category)
                .Include(m => m.cenima)
                .Include(m => m.ActorMovies)
                    .ThenInclude(am => am.actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie != null)
            {
                movie.actors = movie.ActorMovies.Select(am => am.actor).ToList();
            }

            return movie;
        }
        #endregion

        #region Movie Edit
        public async Task<EditMovieVM?> GetEditMovieVMAsync(int id)
        {
            var movie = await _context.movies
                .Include(m => m.Category)
                .Include(m => m.cenima)
                .Include(m => m.ActorMovies)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return null;

            return new EditMovieVM
            {
                Movie = movie,
                Categories = await _context.categories.ToListAsync(),
                Cenimas = await _context.cenimas.ToListAsync(),
                AllActors = await _context.actors.ToListAsync(),
                SelectedActorIds = movie.ActorMovies.Select(am => am.ActorId).ToList()
            };
        }
        public async Task<bool> UpdateMovieAsync(EditMovieVM vm, IFormFile? ImgUrl)
        {
            var movieInDb = await _context.movies
                .Include(m => m.ActorMovies)
                .FirstOrDefaultAsync(m => m.Id == vm.Movie.Id);

            if (movieInDb == null) return false;

            // تحديث الصورة لو تم رفع واحدة جديدة
            if (ImgUrl != null && ImgUrl.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImgUrl.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImgUrl.CopyToAsync(stream);
                }

                // حذف القديمة
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies", movieInDb.ImgUrl);
                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                }

                movieInDb.ImgUrl = fileName;
            }

            // تحديث البيانات
            movieInDb.Name = vm.Movie.Name;
            movieInDb.Description = vm.Movie.Description;
            movieInDb.Price = vm.Movie.Price;
            movieInDb.TrairlerUrl = vm.Movie.TrairlerUrl;
            movieInDb.StartDate = vm.Movie.StartDate;
            movieInDb.EndDate = vm.Movie.EndDate;
            movieInDb.CenimaId = vm.Movie.CenimaId;
            movieInDb.CategoryId = vm.Movie.CategoryId;

            // الممثلين
            _context.ActorMovies.RemoveRange(movieInDb.ActorMovies);
            foreach (var actorId in vm.SelectedActorIds)
            {
                _context.ActorMovies.Add(new ActorMovie
                {
                    ActorId = actorId,
                    MovieId = movieInDb.Id
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Delete Action
        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _context.movies.FindAsync(id);
            if (movie == null) return false;

            // حذف الصورة من السيرفر
            if (!string.IsNullOrEmpty(movie.ImgUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies", movie.ImgUrl);
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }

            _context.movies.Remove(movie);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion


    }

}
