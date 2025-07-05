using System.ComponentModel.DataAnnotations;

namespace Cinema_ETickets.ViewModel
{
    public class LoginVM
    {
        public int Id { get; set; }
        [Required]
        public string UserNameOrEmail { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
