using Microsoft.EntityFrameworkCore;

namespace Cinema_ETickets.Models
{
    [PrimaryKey(nameof(OrderId), nameof(MovieId))]
    public class OrderItem
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
