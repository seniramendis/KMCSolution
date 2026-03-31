using KMC.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(string? category, DateTime? date, string? location)
        {
            try
            {
                // Save search filters to ViewBag so the form remembers what the user typed
                ViewBag.Category = category;
                ViewBag.Date = date?.ToString("yyyy-MM-dd");
                ViewBag.Location = location;

                var events = await _api.GetEventsAsync(category, date, location);
                return View(events);
            }
            catch (Exception ex)
            {
                // Display error message to user
                ViewBag.ErrorMessage = ex.Message;
                return View(new List<KMC.Client.Models.EventViewModel>());
            }
        }
    }
}