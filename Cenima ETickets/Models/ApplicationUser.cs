using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Cinema_ETickets.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Address { get; set; }
        public string FirstName { get; set; } = null!;        
        public string LastName { get; set; } = null!;
        public DateTime? EmailConfirmationSentAt { get; set; }
        public DateTime? PasswordLastChangedAt { get; set; }
        public string? ProfilePicture { get; set; }


    }
}
