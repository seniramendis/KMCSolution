using System.IO;
using KMC.Client.Models;
using KMC.Client.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Client.Controllers
{
    public class EventController : Controller
    {
        private readonly ApiService _api;

        public EventController(ApiService api) => _api = api;

        // --- PUBLIC VIEWS ---
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _api.GetEventAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // --- ORGANIZER VIEWS ---
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");
            return View(new CreateEventViewModel
            {
                Title = string.Empty,
                Description = string.Empty,
                Category = "Music",
                Location = string.Empty,
                Capacity = 100,
                EventDate = DateTime.Now.AddDays(7)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventViewModel model, IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Convert the uploaded image into a Base64 string to send securely via JSON
            if (imageFile != null && imageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                model.ImageUrl = $"data:{imageFile.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
            }

            var result = await _api.CreateEventAsync(model);
            if (result == null)
            {
                ViewBag.Error = "Failed to create event.";
                return View(model);
            }
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");
            var ev = await _api.GetEventAsync(id);
            if (ev == null) return NotFound();

            ViewBag.EventId = id;
            return View(new CreateEventViewModel
            {
                Title = ev.Title ?? string.Empty,
                Description = ev.Description ?? string.Empty,
                Category = ev.Category ?? "Music",
                Location = ev.Location ?? string.Empty,
                EventDate = ev.EventDate,
                Capacity = ev.Capacity,
                ImageUrl = ev.ImageUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateEventViewModel model, IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = id;
                return View(model);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                model.ImageUrl = $"data:{imageFile.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
            }

            var result = await _api.UpdateEventAsync(id, model);
            if (result == null)
            {
                ViewBag.Error = "Failed to update event.";
                ViewBag.EventId = id;
                return View(model);
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteEventAsync(id);
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");
            var events = await _api.GetMyEventsAsync();
            return View(events);
        }

        // --- ATTENDEE VIEWS ---
        [HttpPost]
        public async Task<IActionResult> Register(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Auth");
            var (success, message) = await _api.RegisterForEventAsync(id);
            TempData["Message"] = message;
            TempData["Success"] = success.ToString();
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            await _api.CancelRegistrationAsync(id);
            return RedirectToAction("MyRegistrations");
        }

        public async Task<IActionResult> MyRegistrations()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Auth");
            var list = await _api.GetMyRegistrationsAsync();
            return View(list);
        }
    }
}