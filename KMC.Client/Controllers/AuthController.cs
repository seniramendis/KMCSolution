using KMC.Client.Models;
using KMC.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Client.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _api;

        public AuthController(ApiService api) => _api = api;

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _api.LoginAsync(model);
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Save token and user details in the browser session!
                HttpContext.Session.SetString("JwtToken", response.Token);
                HttpContext.Session.SetString("FullName", response.FullName ?? "");
                HttpContext.Session.SetString("Role", response.Role ?? "");
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _api.RegisterAsync(model);
            if (response != null)
            {
                TempData["Message"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Registration failed. Email might be in use.";
            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Wipe the session to log them out
            return RedirectToAction("Index", "Home");
        }
    }
}