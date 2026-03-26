namespace KMC.Client.Models
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; }
        public int RegisteredCount { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public int OrganizerId { get; set; }
        public int SpotsLeft => Capacity - RegisteredCount;
    }

    public class CreateEventViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);
        public int Capacity { get; set; } = 100;
    }

    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Public";
    }

    public class RegistrationViewModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public EventViewModel Event { get; set; } = new();
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}