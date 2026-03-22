using System;
using System.ComponentModel.DataAnnotations;

namespace KMC.API.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }

        public int OrganizerId { get; set; }
    }
}