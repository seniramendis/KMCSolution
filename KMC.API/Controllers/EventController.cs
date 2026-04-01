using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KMC.API.DTOs;
using KMC.API.Data;
using KMC.API.Models;
using System.Linq;
using System.Threading.Tasks;

namespace KMC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var organizerId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            if (organizerId == 0) return Unauthorized("Invalid user ID.");

            var newEvent = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                EventDate = dto.EventDate,
                Location = dto.Location,
                Capacity = dto.Capacity,
                OrganizerId = organizerId,
                ImageUrl = dto.ImageUrl ?? string.Empty
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event created successfully!" });
        }

        [HttpGet("{id}")]
        public IActionResult GetEvent(int id)
        {
            var ev = (from e in _context.Events
                      join u in _context.Users on e.OrganizerId equals u.UserId
                      where e.EventId == id
                      select new
                      {
                          e.EventId,
                          e.Title,
                          e.Description,
                          e.Category,
                          e.EventDate,
                          e.Location,
                          e.Capacity,
                          e.ImageUrl,
                          OrganizerName = u.FullName,
                          // The count has been added here!
                          RegisteredCount = _context.Registrations.Count(r => r.EventId == e.EventId)
                      }).FirstOrDefault();

            if (ev == null) return NotFound();
            return Ok(ev);
        }

        [HttpGet]
        public IActionResult GetAllEvents([FromQuery] string? category, [FromQuery] DateTime? date, [FromQuery] string? location)
        {
            var query = from e in _context.Events
                        join u in _context.Users on e.OrganizerId equals u.UserId
                        select new
                        {
                            e.EventId,
                            e.Title,
                            e.Description,
                            e.Category,
                            e.EventDate,
                            e.Location,
                            e.Capacity,
                            e.ImageUrl,
                            OrganizerName = u.FullName,
                            // The count has been added here!
                            RegisteredCount = _context.Registrations.Count(r => r.EventId == e.EventId)
                        };

            if (!string.IsNullOrEmpty(category)) query = query.Where(e => e.Category == category);
            if (date.HasValue) query = query.Where(e => e.EventDate.Date == date.Value.Date);
            if (!string.IsNullOrEmpty(location)) query = query.Where(e => e.Location.Contains(location));

            return Ok(query.ToList());
        }

        [HttpGet("my")]
        [Authorize]
        public IActionResult GetMyEvents()
        {
            var organizerId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            if (organizerId == 0) return Unauthorized("Invalid user ID.");

            var myEvents = (from e in _context.Events
                            join u in _context.Users on e.OrganizerId equals u.UserId
                            where e.OrganizerId == organizerId
                            select new
                            {
                                e.EventId,
                                e.Title,
                                e.Description,
                                e.Category,
                                e.EventDate,
                                e.Location,
                                e.Capacity,
                                e.ImageUrl,
                                OrganizerName = u.FullName,
                                // The count has been added here!
                                RegisteredCount = _context.Registrations.Count(r => r.EventId == e.EventId)
                            }).ToList();

            return Ok(myEvents);
        }

        // NEW: Gets the specific Guest List for an Organizer
        [HttpGet("{eventId}/attendees")]
        [Authorize]
        public IActionResult GetEventAttendees(int eventId)
        {
            var organizerId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            if (organizerId == 0) return Unauthorized("Invalid user ID.");

            var ev = _context.Events.FirstOrDefault(e => e.EventId == eventId);
            if (ev == null || ev.OrganizerId != organizerId)
                return Forbid("You are not the organizer of this event.");

            var attendees = (from r in _context.Registrations
                             join u in _context.Users on r.UserId equals u.UserId
                             where r.EventId == eventId
                             select new
                             {
                                 u.FullName,
                                 u.Email,
                                 r.RegistrationDate
                             }).ToList();

            return Ok(attendees);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var organizerId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            if (organizerId == 0) return Unauthorized("Invalid user ID.");

            var eventToDelete = await _context.Events.FindAsync(id);
            if (eventToDelete == null) return NotFound("Event not found.");

            if (eventToDelete.OrganizerId != organizerId) return Forbid("You can only delete your own events.");

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully" });
        }
    }
}