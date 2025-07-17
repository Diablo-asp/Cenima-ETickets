using System.Linq;
using System.Linq.Expressions;
using Cinema_ETickets.Data;
using Cinema_ETickets.Models;
using Cinema_ETickets.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Cinema_ETickets.Repositories
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {       
            _context = context;
        }

        public async Task<bool> CreateRangeAsync(List<OrderItem> entities)
        {
            try
            {
                _context.OrderItems.AddRange(entities);
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
