﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cinema_ETickets.ViewModel
{
    public class RegisterVM
    {
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;        
        public string? Address { get; set; }
        public List<SelectListItem>? Roles { get; set; }
        
    }
}
