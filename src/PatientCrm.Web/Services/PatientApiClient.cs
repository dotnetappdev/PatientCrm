using System.Net.Http.Headers;
using System.Net.Http.Json;
using PatientCrm.Core.Entities;

namespace PatientCrm.Web.Services;

/// <summary>Typed HTTP client that wraps all calls to the PatientCRM API.</summary>
public class PatientApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PatientApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    // Attach the stored JWT to every outgoing request
    private void SetAuthHeader()
    {
        var token = _httpContextAccessor.HttpContext?.User.FindFirst("api_token")?.Value;
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        else
            _http.DefaultRequestHeaders.Authorization = null;
    }

    // Auth

    public async Task<LoginResult?> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResult>();
    }

    public async Task<TwoFactorResult?> VerifyTwoFactorAsync(string partialToken, string code)
    {
        var response = await _http.PostAsJsonAsync("api/auth/2fa/verify", new { partialToken, code });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TwoFactorResult>();
    }

    // Dashboard

    public async Task<DashboardStats?> GetDashboardStatsAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<DashboardStats>("api/dashboard/stats");
    }

    // Patients

    public async Task<PagedPatientResult?> GetPatientsAsync(string? search, int page, int pageSize)
    {
        SetAuthHeader();
        var url = $"api/patients?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        var patients = await response.Content.ReadFromJsonAsync<List<Patient>>();
        return new PagedPatientResult { Patients = patients ?? [], Total = patients?.Count ?? 0 };
    }

    public async Task<Patient?> GetPatientAsync(Guid id)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<Patient>($"api/patients/{id}");
    }

    public async Task<Patient?> CreatePatientAsync(Patient patient)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/patients", patient);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Patient>();
    }

    public async Task<bool> UpdatePatientAsync(Guid id, Patient patient)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/patients/{id}", patient);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePatientAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/patients/{id}");
        return response.IsSuccessStatusCode;
    }

    // Clinical Notes

    public async Task<bool> AddNoteAsync(Guid patientId, ClinicalNote note)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/notes?patientId={patientId}", note);
        return response.IsSuccessStatusCode;
    }

    // Appointments

    public async Task<List<Appointment>?> GetAppointmentsAsync(DateTime? date)
    {
        SetAuthHeader();
        var url = "api/appointments";
        if (date.HasValue)
            url += $"?date={date.Value:yyyy-MM-dd}";
        return await _http.GetFromJsonAsync<List<Appointment>>(url);
    }

    // Admin

    public async Task<AdminStats?> GetAdminStatsAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<AdminStats>("api/admin/stats");
    }

    public async Task<List<Tenant>?> GetTenantsAsync(string? search, string? clientType)
    {
        SetAuthHeader();
        var url = "api/admin/tenants";
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(clientType)) qs.Add($"clientType={clientType}");
        if (qs.Count > 0) url += "?" + string.Join("&", qs);
        return await _http.GetFromJsonAsync<List<Tenant>>(url);
    }

    public async Task<Tenant?> GetTenantAsync(Guid id)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<Tenant>($"api/admin/tenants/{id}");
    }

    public async Task<Tenant?> CreateTenantAsync(Tenant tenant)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/admin/tenants", tenant);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Tenant>();
    }

    public async Task<bool> UpdateTenantAsync(Guid id, Tenant tenant)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/admin/tenants/{id}", tenant);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleTenantAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.PostAsync($"api/admin/tenants/{id}/toggle", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<UserDto>?> GetUsersAsync(Guid? tenantId)
    {
        SetAuthHeader();
        var url = "api/admin/users";
        if (tenantId.HasValue) url += $"?tenantId={tenantId}";
        return await _http.GetFromJsonAsync<List<UserDto>>(url);
    }

    // Image upload (multipart)
    public async Task<bool> UploadImageAsync(Guid patientId, IFormFile file, string title, string imageType, string? description, Guid? clinicalNoteId)
    {
        SetAuthHeader();
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        content.Add(new StreamContent(stream), "file", file.FileName);
        content.Add(new StringContent(title), "title");
        content.Add(new StringContent(imageType), "imageType");
        if (!string.IsNullOrEmpty(description)) content.Add(new StringContent(description), "description");
        if (clinicalNoteId.HasValue) content.Add(new StringContent(clinicalNoteId.ToString()!), "clinicalNoteId");

        var response = await _http.PostAsync($"api/images?patientId={patientId}", content);
        return response.IsSuccessStatusCode;
    }
}

// DTOs for API responses

public class LoginResult
{
    public bool RequiresTwoFactor { get; set; }
    public string? PartialToken { get; set; }
    public string? Token { get; set; }
    public UserInfo? User { get; set; }
    public string? Message { get; set; }
}

public class TwoFactorResult
{
    public string? Token { get; set; }
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public Guid TenantId { get; set; }
    public string? Theme { get; set; }
    public bool TwoFactorEnabled { get; set; }
}

public class DashboardStats
{
    public int TotalPatients { get; set; }
    public int TodayAppointments { get; set; }
    public int ActivePrescriptions { get; set; }
    public int ActiveAlerts { get; set; }
    public List<PatientSummary>? RecentPatients { get; set; }
    public List<AppointmentSummary>? UpcomingAppointments { get; set; }
}

public class PatientSummary
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? NhsNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AppointmentSummary
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Title { get; set; }
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
}

public class PagedPatientResult
{
    public List<Patient> Patients { get; set; } = [];
    public int Total { get; set; }
}

public class AdminStats
{
    public int TenantCount { get; set; }
    public int UserCount { get; set; }
    public int PatientCount { get; set; }
    public int GpCount { get; set; }
    public int DentalCount { get; set; }
    public int HospitalCount { get; set; }
    public List<TenantSummary>? Tenants { get; set; }
}

public class TenantSummary
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public bool IsActive { get; set; }
    public string? Email { get; set; }
    public string? OdsCode { get; set; }
    public string? City { get; set; }
    public string? Postcode { get; set; }
    public PatientCrm.Core.Enums.ClientType ClientType { get; set; }
    public PatientCrm.Core.Enums.TenantType TenantType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public Guid TenantId { get; set; }
    public bool IsActive { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? GmcNumber { get; set; }
    public string? GdcNumber { get; set; }
    public string? Title { get; set; }
    public string? TenantName { get; set; }
}
