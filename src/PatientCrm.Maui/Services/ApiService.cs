using System.Net.Http.Headers;
using System.Net.Http.Json;
using PatientCrm.Maui.Models;

namespace PatientCrm.Maui.Services;

/// <summary>Typed HTTP client that wraps all calls to the PatientCRM REST API.</summary>
public class ApiService
{
    private readonly HttpClient _http;
    private readonly IAuthService _authService;

    public ApiService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _http = httpClientFactory.CreateClient("PatientCrmApi");
        _authService = authService;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _authService.GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    // ── Dashboard ────────────────────────────────────────────────────────────
    public async Task<DashboardStats?> GetDashboardStatsAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<DashboardStats>("api/dashboard/stats");
    }

    // ── Patients ─────────────────────────────────────────────────────────────
    public async Task<PagedPatientResult?> GetPatientsAsync(string? search, int page = 1, int pageSize = 20)
    {
        await SetAuthHeaderAsync();
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
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<Patient>($"api/patients/{id}");
    }

    public async Task<Patient?> CreatePatientAsync(Patient patient)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/patients", patient);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Patient>();
    }

    public async Task<bool> UpdatePatientAsync(Guid id, Patient patient)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/patients/{id}", patient);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePatientAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _http.DeleteAsync($"api/patients/{id}");
        return response.IsSuccessStatusCode;
    }

    // ── Clinical Notes ───────────────────────────────────────────────────────
    public async Task<bool> AddNoteAsync(Guid patientId, ClinicalNote note)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync($"api/notes?patientId={patientId}", note);
        return response.IsSuccessStatusCode;
    }

    // ── Appointments ─────────────────────────────────────────────────────────
    public async Task<List<Appointment>?> GetAppointmentsAsync(DateTime? date = null)
    {
        await SetAuthHeaderAsync();
        var url = "api/appointments";
        if (date.HasValue) url += $"?date={date.Value:yyyy-MM-dd}";
        return await _http.GetFromJsonAsync<List<Appointment>>(url);
    }

    public async Task<List<Appointment>?> GetPatientAppointmentsAsync(Guid patientId)
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<Appointment>>($"api/patients/{patientId}/appointments");
    }

    public async Task<Appointment?> CreateAppointmentAsync(Appointment appointment)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/appointments", appointment);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Appointment>();
    }

    public async Task<bool> UpdateAppointmentAsync(Guid id, Appointment appointment)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/appointments/{id}", appointment);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CancelAppointmentAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _http.DeleteAsync($"api/appointments/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<VideoLinkResult?> GenerateVideoLinkAsync(Guid appointmentId, string platform, bool enablePasscode)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync($"api/appointments/{appointmentId}/generate-video-link",
            new { Platform = platform, EnablePasscode = enablePasscode });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VideoLinkResult>();
    }

    // ── Prescriptions ────────────────────────────────────────────────────────
    public async Task<List<Prescription>?> GetPatientPrescriptionsAsync(Guid patientId)
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<Prescription>>($"api/patients/{patientId}/prescriptions");
    }

    public async Task<Prescription?> CreatePrescriptionAsync(Prescription prescription)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/prescriptions", prescription);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Prescription>();
    }

    public async Task<bool> UpdatePrescriptionAsync(Guid id, Prescription prescription)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/prescriptions/{id}", prescription);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePrescriptionAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _http.DeleteAsync($"api/prescriptions/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool Ok, string? Error)> ReorderPrescriptionAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsync($"api/prescriptions/{id}/reorder", null);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await response.Content.ReadAsStringAsync());
    }

    // ── Letters ──────────────────────────────────────────────────────────────
    public async Task<List<Letter>?> GetLettersAsync(Guid? patientId = null)
    {
        await SetAuthHeaderAsync();
        var url = "api/letters";
        if (patientId.HasValue) url += $"?patientId={patientId}";
        return await _http.GetFromJsonAsync<List<Letter>>(url);
    }

    public async Task<Letter?> GetLetterAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<Letter>($"api/letters/{id}");
    }

    public async Task<Letter?> CreateLetterAsync(Letter letter)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/letters", letter);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Letter>();
    }

    public async Task<bool> UpdateLetterAsync(Guid id, Letter letter)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/letters/{id}", letter);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendLetterAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsync($"api/letters/{id}/send", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<LetterTemplate>?> GetLetterTemplatesAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<LetterTemplate>>("api/letters/templates");
    }

    // ── Departments ──────────────────────────────────────────────────────────
    public async Task<List<Department>?> GetDepartmentsAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<Department>>("api/departments");
    }

    public async Task<Department?> GetDepartmentAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<Department>($"api/departments/{id}");
    }

    public async Task<Department?> CreateDepartmentAsync(Department department)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/departments", department);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Department>();
    }

    public async Task<bool> UpdateDepartmentAsync(Guid id, Department department)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/departments/{id}", department);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<PatientAdmission>?> GetPatientAdmissionsAsync(Guid? patientId = null, Guid? departmentId = null)
    {
        await SetAuthHeaderAsync();
        var url = "api/departments/admissions?";
        if (patientId.HasValue) url += $"patientId={patientId}&";
        if (departmentId.HasValue) url += $"departmentId={departmentId}";
        return await _http.GetFromJsonAsync<List<PatientAdmission>>(url);
    }

    public async Task<PatientAdmission?> CreateAdmissionAsync(PatientAdmission admission)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/departments/admissions", admission);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PatientAdmission>();
    }

    public async Task<bool> UpdateAdmissionAsync(Guid id, PatientAdmission admission)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/departments/admissions/{id}", admission);
        return response.IsSuccessStatusCode;
    }

    // ── Dental ───────────────────────────────────────────────────────────────
    public async Task<DentalRecord?> GetDentalRecordAsync(Guid patientId)
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<DentalRecord>($"api/dental/{patientId}");
    }

    public async Task<bool> UpdateDentalRecordAsync(Guid id, DentalRecord record)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/dental/{id}", record);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ToothRecord>?> GetToothRecordsAsync(Guid patientId)
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<ToothRecord>>($"api/dental/{patientId}/teeth");
    }

    // ── Admin ─────────────────────────────────────────────────────────────────
    public async Task<AdminStats?> GetAdminStatsAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<AdminStats>("api/admin/stats");
    }

    public async Task<List<TenantSummary>?> GetTenantsAsync(string? search = null, string? clientType = null)
    {
        await SetAuthHeaderAsync();
        var url = "api/admin/tenants";
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(clientType)) qs.Add($"clientType={clientType}");
        if (qs.Count > 0) url += "?" + string.Join("&", qs);
        return await _http.GetFromJsonAsync<List<TenantSummary>>(url);
    }

    public async Task<List<UserDto>?> GetUsersAsync(Guid? tenantId = null)
    {
        await SetAuthHeaderAsync();
        var url = "api/admin/users";
        if (tenantId.HasValue) url += $"?tenantId={tenantId}";
        return await _http.GetFromJsonAsync<List<UserDto>>(url);
    }

    public async Task<bool> ToggleTenantAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsync($"api/admin/tenants/{id}/toggle", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleUserAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsync($"api/admin/users/{id}/toggle", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetUserPasswordAsync(Guid id, string newPassword)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync($"api/admin/users/{id}/reset-password", new { newPassword });
        return response.IsSuccessStatusCode;
    }

    // ── Patient Portal ───────────────────────────────────────────────────────
    public async Task<Patient?> GetMyPortalRecordAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<Patient>("api/portal/me");
    }

    public async Task<List<Appointment>?> GetMyPortalAppointmentsAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<Appointment>>("api/portal/appointments");
    }

    public async Task<List<Prescription>?> GetMyPortalPrescriptionsAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<Prescription>>("api/portal/prescriptions");
    }

    public async Task<List<ClinicalNote>?> GetMyPortalNotesAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<ClinicalNote>>("api/portal/notes");
    }

    public async Task<List<Letter>?> GetMyPortalLettersAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<Letter>>("api/portal/me/letters");
    }

    public async Task<bool> EnablePatientPortalAccessAsync(Guid patientId)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsync($"api/patients/{patientId}/portal-access", null);
        return response.IsSuccessStatusCode;
    }
}
