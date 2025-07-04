using System.Linq;
using System.Linq.Expressions;
using Cinema_ETickets.Data;
using Cinema_ETickets.Models;
using Cinema_ETickets.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Cinema_ETickets.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {       
            _context = context;
        }
    }
}
