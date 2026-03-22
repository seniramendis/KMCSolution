using System;
using System.ComponentModel.DataAnnotations;

namespace KMC.API.DTOs
{
    // 1. Used when someone creates a new account
    public class UserRegisterDto
    {
        [Required] public string FullName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }
        [Required] public string Role { get; set; } // "Public" or "Organizer"
    }

    // 2. Used when someone logs in
    public class UserLoginDto
    {
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }
    }

    // 3. Used when an Organizer creates a new event
    public class EventCreateDto
    {
        [Required] public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public int OrganizerId { get; set; }
    }
}