namespace Cinema_ETickets.ViewModel
{
    public class UserTicketVM
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        public string MovieName { get; set; }
        public string MovieImage { get; set; }
        public string MovieStatus { get; set; }  // حالة الفيلم
        public string OrderStatus { get; set; }  // حالة الشراء
        public DateTime StartDate { get; set; }  // موعد العرض
        public DateTime EndDate { get; set; }    // نهاية العرض

        public string CinemaName { get; set; }   // اسم السينما
        public string CategoryName { get; set; } // اسم الكاتجوري

        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
