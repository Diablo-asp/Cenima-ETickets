using System.ComponentModel.DataAnnotations;

namespace Cinema_ETickets.ViewModel
{
    public class ResendConfirmEmailVM
    {
        public int Id { get; set; }
        [Required]
        public string EmailOrUserName { get; set; }
    }
}
