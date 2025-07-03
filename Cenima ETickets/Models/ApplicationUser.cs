using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Cenima_ETickets.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Address { get; set; }
        public string FirstName { get; set; } = null!;        
        public string LastName { get; set; } = null!;
    }
}
