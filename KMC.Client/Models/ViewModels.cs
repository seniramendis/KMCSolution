using System;
using System.ComponentModel.DataAnnotations;

namespace KMC.Client.Models
{
    // ==========================================
    // 1. AUTHENTICATION MODELS
    // ==========================================
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Public";
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }

    // ==========================================
    // 2. EVENT MODELS
    // ==========================================
    public class EventViewModel
    {
        public int EventId { get; set; }
        public int Id => EventId;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; }
        public string? ImageUrl { get; set; }

        public int RegisteredCount { get; set; }
        public int SpotsLeft => Capacity - RegisteredCount;
        public string OrganizerName { get; set; } = "Organizer";
    }

    public class CreateEventViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }
        public DateTime Date { get => EventDate; set => EventDate = value; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public int Capacity { get; set; }
        public int MaxAttendees { get => Capacity; set => Capacity = value; }

        public string? ImageUrl { get; set; }
    }

    // ==========================================
    // 3. REGISTRATION MODELS
    // ==========================================
    public class RegistrationViewModel
    {
        public int RegistrationId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}