using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KMC.API.DTOs;
using KMC.API.Data;
using KMC.API.Models;

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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // THE FIX: Look for the exact "id" label that your AuthController created!
            var organizerId = int.Parse(User.FindFirst("id")?.Value ?? "0");

            if (organizerId == 0)
                return Unauthorized("Invalid user ID. The token does not contain your ID.");

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

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventId }, newEvent);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
                return NotFound();

            return Ok(@event);
        }

        [HttpGet]
        public IActionResult GetAllEvents()
        {
            var events = _context.Events.ToList();
            return Ok(events);
        }
    }
}