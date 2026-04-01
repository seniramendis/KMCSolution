using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KMC.API.Data;
using KMC.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KMC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistrationsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. REGISTER FOR AN EVENT
        [HttpPost("{eventId}")]
        [Authorize]
        public async Task<IActionResult> Register(int eventId)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            if (userId == 0) return Unauthorized("Invalid user ID.");

            // Check if the event exists
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });

            // Check if the user is already registered
            var existing = await _context.Registrations.FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId);
            if (existing != null) return BadRequest(new { message = "You are already registered for this event." });

            // Check if it is sold out!
            var currentRegistrations = await _context.Registrations.CountAsync(r => r.EventId == eventId);
            if (currentRegistrations >= ev.Capacity) return BadRequest(new { message = "This event is sold out." });

            // Create the ticket
            var reg = new Registration
            {
                UserId = userId,
                EventId = eventId,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Registrations.Add(reg);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully registered!" });
        }

        // 2. GET MY TICKETS (For the Attendee Dashboard)
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyRegistrations()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized("Invalid user ID.");

            // Join the tables to get the Event details for the tickets
            var myRegs = await (from r in _context.Registrations
                                join e in _context.Events on r.EventId equals e.EventId
                                join u in _context.Users on e.OrganizerId equals u.UserId
                                where r.UserId == userId
                                select new
                                {
                                    r.RegistrationId,
                                    r.RegistrationDate,
                                    e.EventId,
                                    e.Title,
                                    e.EventDate,
                                    e.Location,
                                    e.ImageUrl,
                                    OrganizerName = u.FullName
                                }).ToListAsync();

            return Ok(myRegs);
        }

        // 3. CANCEL A TICKET
        [HttpDelete("{eventId}")]
        [Authorize]
        public async Task<IActionResult> CancelRegistration(int eventId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized("Invalid user ID.");

            var reg = await _context.Registrations.FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId);
            if (reg == null) return NotFound(new { message = "Registration not found." });

            _context.Registrations.Remove(reg);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration cancelled." });
        }
    }
}