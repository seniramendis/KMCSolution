using System;
using System.ComponentModel.DataAnnotations;

namespace KMC.API.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public int OrganizerId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }
}