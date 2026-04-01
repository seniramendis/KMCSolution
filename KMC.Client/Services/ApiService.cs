using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using KMC.Client.Models;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace KMC.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public ApiService(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        // --- Auth ---
        public async Task<AuthResponse?> LoginAsync(LoginViewModel model)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/Auth/login", model);
                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AuthResponse>(content, _json);
                }
                var errorMsg = await res.Content.ReadAsStringAsync();
                throw new Exception($"API Rejected: {errorMsg}");
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/Auth/register", model);
                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AuthResponse>(content, _json);
                }
                var errorMsg = await res.Content.ReadAsStringAsync();
                throw new Exception(errorMsg);
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        // --- Events ---
        public async Task<List<EventViewModel>> GetEventsAsync(string? category = null, DateTime? date = null, string? location = null)
        {
            try
            {
                var url = "api/events?";
                if (!string.IsNullOrEmpty(category)) url += $"category={Uri.EscapeDataString(category)}&";
                if (date.HasValue) url += $"date={date.Value.ToString("yyyy-MM-dd")}&";
                if (!string.IsNullOrEmpty(location)) url += $"location={Uri.EscapeDataString(location)}";

                url = url.TrimEnd('&', '?');

                return await GetAsync<List<EventViewModel>>(url) ?? new List<EventViewModel>();
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        public async Task<EventViewModel?> GetEventAsync(int id)
        {
            try
            {
                return await GetAsync<EventViewModel>($"api/events/{id}");
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        public async Task<EventViewModel?> CreateEventAsync(CreateEventViewModel model)
        {
            try
            {
                return await PostAsync<EventViewModel>("api/events", model, true);
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        public async Task<EventViewModel?> UpdateEventAsync(int id, CreateEventViewModel model)
        {
            try
            {
                return await PutAsync<EventViewModel>($"api/events/{id}", model, true);
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            try
            {
                AttachToken();
                var res = await _http.DeleteAsync($"api/events/{id}");
                return res.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        public async Task<List<EventViewModel>> GetMyEventsAsync()
        {
            try
            {
                return await GetAsync<List<EventViewModel>>("api/events/my", true) ?? new();
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        // --- Registrations ---
        public async Task<(bool Success, string Message)> RegisterForEventAsync(int eventId)
        {
            try
            {
                AttachToken();
                var res = await _http.PostAsync($"api/registrations/{eventId}", null);
                if (res.IsSuccessStatusCode) return (true, "Registered successfully!");
                try
                {
                    var err = JsonSerializer.Deserialize<Dictionary<string, string>>(await res.Content.ReadAsStringAsync(), _json);
                    return (false, err?["message"] ?? "Failed to register.");
                }
                catch { return (false, "Failed to register."); }
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        public async Task<bool> CancelRegistrationAsync(int eventId)
        {
            try
            {
                AttachToken();
                var res = await _http.DeleteAsync($"api/registrations/{eventId}");
                return res.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        public async Task<List<RegistrationViewModel>> GetMyRegistrationsAsync()
        {
            try
            {
                return await GetAsync<List<RegistrationViewModel>>("api/registrations/my", true) ?? new();
            }
            catch (HttpRequestException)
            {
                throw new Exception("CONNECTION ERROR: Cannot reach the API. Are both projects running?");
            }
        }

        // --- Helpers ---
        private async Task<T?> GetAsync<T>(string url, bool withAuth = false)
        {
            if (withAuth) AttachToken();
            var res = await _http.GetAsync(url);
            return res.IsSuccessStatusCode ? JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(), _json) : default;
        }

        private async Task<T?> PostAsync<T>(string url, object body, bool withAuth = false)
        {
            if (withAuth) AttachToken();
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(url, content);

            if (res.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(), _json);
            }

            var errorMsg = await res.Content.ReadAsStringAsync();
            throw new Exception($"API REJECTED IT: {errorMsg}");
        }

        private async Task<T?> PutAsync<T>(string url, object body, bool withAuth = false)
        {
            if (withAuth) AttachToken();
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var res = await _http.PutAsync(url, content);
            return res.IsSuccessStatusCode ? JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(), _json) : default;
        }

        private void AttachToken()
        {
            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token)) _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // THE FIX: Updated the link spelling and attached the Organizer token!
        public async Task<List<AttendeeViewModel>> GetEventAttendeesAsync(int eventId)
        {
            try
            {
                AttachToken();
                var res = await _http.GetAsync($"api/events/{eventId}/attendees");

                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<AttendeeViewModel>>(content, _json) ?? new List<AttendeeViewModel>();
                }
                return new List<AttendeeViewModel>();
            }
            catch
            {
                return new List<AttendeeViewModel>();
            }
        }
    }

    public class AttendeeViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
    }
}