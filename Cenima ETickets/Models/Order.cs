namespace Cinema_ETickets.Models
{
    public enum OrderStatus
    {
        pending,
        procession,
        shipped,
        inWay,
        Completed,
        Canceled,
        Refunded
    }

    public enum PaymentMethod
    {
        Visa,
        Cash
    }

    public class Order
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public DateTime Date { get; set; }
        public DateTime ShippedDate { get; set; }
        public double TotalPrice { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public string? Carrier { get; set; }
        public int? CarrierId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? PaymentId { get; set; }
        public string? SessionId { get; set; }
    }
}
