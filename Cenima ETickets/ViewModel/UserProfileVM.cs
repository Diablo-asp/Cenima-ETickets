using System.ComponentModel.DataAnnotations;

namespace Cinema_ETickets.ViewModel
{
    public class UserProfileVM
    {
        public string Id { get; set; } = null!;
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string? ConfirmPassword { get; set; }

    }
}
