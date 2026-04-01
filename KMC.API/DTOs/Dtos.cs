using System;
using System.ComponentModel.DataAnnotations;

namespace KMC.API.DTOs
{
    // 1. Used when someone creates a new account
    public class UserRegisterDto
    {
        [Required] public required string FullName { get; set; }
        [Required, EmailAddress] public required string Email { get; set; }
        [Required] public required string Password { get; set; }
        [Required] public required string Role { get; set; } // "Public" or "Organizer"
    }

    // 2. Used when someone logs in
    public class UserLoginDto
    {
        [Required, EmailAddress] public required string Email { get; set; }
        [Required] public required string Password { get; set; }
    }

    // 3. Used when an Organizer creates a new event
    public class EventCreateDto
    {
        [Required] public required string Title { get; set; }
        [Required] public required string Description { get; set; }
        [Required] public required string Category { get; set; }
        public DateTime EventDate { get; set; }
        [Required] public required string Location { get; set; }
        public int Capacity { get; set; }
        public int OrganizerId { get; set; }
        public string? ImageUrl { get; set; }
    }
}