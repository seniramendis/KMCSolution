using System;
using System.IO;
using System.Threading.Tasks;
using KMC.Client.Models;
using KMC.Client.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Client.Controllers
{
    public class EventController : Controller
    {
        private readonly ApiService _api;
        private readonly IWebHostEnvironment _env;

        public EventController(ApiService api, IWebHostEnvironment env)
        {
            _api = api;
            _env = env;
        }

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
            return View(new CreateEventViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventViewModel model, IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string uploadsFolder = Path.Combine(webRootPath, "uploads");

                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = "/uploads/" + uniqueFileName;
                }

                var result = await _api.CreateEventAsync(model);
                if (result == null) { ViewBag.Error = "Failed to create event."; return View(model); }

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"ERROR: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");
            var ev = await _api.GetEventAsync(id);
            if (ev == null) return NotFound();

            ViewBag.EventId = id;
            return View(new CreateEventViewModel
            {
                Title = ev.Title,
                Description = ev.Description,
                Category = ev.Category,
                Location = ev.Location,
                EventDate = ev.EventDate,
                Capacity = ev.Capacity,
                ImageUrl = ev.ImageUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateEventViewModel model, IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string uploadsFolder = Path.Combine(webRootPath, "uploads");

                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = "/uploads/" + uniqueFileName;
                }

                var result = await _api.UpdateEventAsync(id, model);
                if (result == null) { ViewBag.Error = "Failed to update event."; ViewBag.EventId = id; return View(model); }
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"ERROR: {ex.Message}";
                ViewBag.EventId = id;
                return View(model);
            }
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
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Auth");
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