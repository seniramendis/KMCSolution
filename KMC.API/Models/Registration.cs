using System;
using System.ComponentModel.DataAnnotations;

namespace KMC.API.Models
{
    public class Registration
    {
        [Key]
        public int RegistrationId { get; set; }

        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}