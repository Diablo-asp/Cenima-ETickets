using System.Linq;
using System.Linq.Expressions;
using Cinema_ETickets.Data;
using Cinema_ETickets.Models;
using Cinema_ETickets.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Cinema_ETickets.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContext _context;// = new();
        private DbSet<T> _db {  get; set; }

        public Repository(ApplicationDbContext context)
        {
            _context = context;
             _db = _context.Set<T>();
        }

        public async Task<bool> CreateAsync(T entity)
        {
            try
            {
                await _db.AddAsync(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
                return false;
            }
        }
        public async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                _db.Update(entity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            try
            {
                _db.Remove(entity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
                return false;
            }
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>?
            expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            IQueryable<T> entities = _db;

            if (expression != null)
            {
                entities = entities.Where(expression);
            }

            if (includes != null)
            {
                foreach (var item in includes)
                {
                    entities = entities.Include(item);
                }
            }

            if (!tracked)
            {
                entities = entities.AsNoTracking();
            }
            return (await entities.ToListAsync());
        }

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>>?
            expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            return ( await GetAsync(expression, includes, tracked)).FirstOrDefault();
        }

        public async Task<bool> CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
                return false;
            }
        }
    }
}
