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

            try
            {
                var response = await _api.LoginAsync(model);
                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    
                    HttpContext.Session.SetString("JwtToken", response.Token);
                    HttpContext.Session.SetString("FullName", response.FullName ?? "");
                    HttpContext.Session.SetString("Role", response.Role ?? "");
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }
            catch (Exception)
            {
                
                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                
                var response = await _api.RegisterAsync(model);

                
                var loginResponse = await _api.LoginAsync(new LoginViewModel
                {
                    Email = model.Email,
                    Password = model.Password
                });

                
                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    HttpContext.Session.SetString("JwtToken", loginResponse.Token);
                    HttpContext.Session.SetString("FullName", loginResponse.FullName ?? "");
                    HttpContext.Session.SetString("Role", loginResponse.Role ?? "");

                    
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Index", "Home");
        }
    }
}