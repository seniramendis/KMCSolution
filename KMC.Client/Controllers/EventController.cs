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
        private readonly IWebHostEnvironment _env; // NEW: Gives us access to the wwwroot folder

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
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");

            try
            {
                // NO IMAGES. Just send the raw text straight to the API!
                var result = await _api.CreateEventAsync(model);

                if (result == null)
                {
                    ViewBag.Error = "Failed to create event.";
                    return View(model);
                }

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"ERROR: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateEventViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Organizer") return RedirectToAction("Login", "Auth");

            try
            {
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