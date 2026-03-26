using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KMC.Client.Models;

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
            => await PostAsync<AuthResponse>("api/auth/login", model);

        public async Task<AuthResponse?> RegisterAsync(RegisterViewModel model)
            => await PostAsync<AuthResponse>("api/auth/register", model);

        // --- Events ---
        public async Task<List<EventViewModel>> GetEventsAsync(string? category = null, DateTime? date = null, string? location = null)
        {
            var query = BuildQuery(new Dictionary<string, string?> { ["category"] = category, ["date"] = date?.ToString("yyyy-MM-dd"), ["location"] = location });
            return await GetAsync<List<EventViewModel>>($"api/events{query}") ?? new();
        }

        public async Task<EventViewModel?> GetEventAsync(int id) => await GetAsync<EventViewModel>($"api/events/{id}");

        public async Task<EventViewModel?> CreateEventAsync(CreateEventViewModel model) => await PostAsync<EventViewModel>("api/events", model, true);

        public async Task<EventViewModel?> UpdateEventAsync(int id, CreateEventViewModel model) => await PutAsync<EventViewModel>($"api/events/{id}", model, true);

        public async Task<bool> DeleteEventAsync(int id)
        {
            AttachToken();
            var res = await _http.DeleteAsync($"api/events/{id}");
            return res.IsSuccessStatusCode;
        }

        public async Task<List<EventViewModel>> GetMyEventsAsync() => await GetAsync<List<EventViewModel>>("api/events/my", true) ?? new();

        // --- Registrations ---
        public async Task<(bool Success, string Message)> RegisterForEventAsync(int eventId)
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

        public async Task<bool> CancelRegistrationAsync(int eventId)
        {
            AttachToken();
            var res = await _http.DeleteAsync($"api/registrations/{eventId}");
            return res.IsSuccessStatusCode;
        }

        public async Task<List<RegistrationViewModel>> GetMyRegistrationsAsync() => await GetAsync<List<RegistrationViewModel>>("api/registrations/my", true) ?? new();

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
            return res.IsSuccessStatusCode ? JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(), _json) : default;
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

        private static string BuildQuery(Dictionary<string, string?> params_)
        {
            var parts = params_.Where(p => !string.IsNullOrEmpty(p.Value)).Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}");
            var q = string.Join("&", parts);
            return string.IsNullOrEmpty(q) ? "" : "?" + q;
        }
    }
}