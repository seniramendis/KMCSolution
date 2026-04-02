using System;
using System.ComponentModel.DataAnnotations;

namespace KMC.API.DTOs
{
    
    public class UserRegisterDto
    {
        [Required] public required string FullName { get; set; }
        [Required, EmailAddress] public required string Email { get; set; }
        [Required] public required string Password { get; set; }
        [Required] public required string Role { get; set; } 
    }

    
    public class UserLoginDto
    {
        [Required, EmailAddress] public required string Email { get; set; }
        [Required] public required string Password { get; set; }
    }

    
    public class EventCreateDto
    {
        [Required] public required string Title { get; set; }
        [Required] public required string Description { get; set; }
        [Required] public required string Category { get; set; }
        public DateTime EventDate { get; set; }
        [Required] public required string Location { get; set; }
        public int Capacity { get; set; }


        public string? ImageUrl { get; set; }
    }
}