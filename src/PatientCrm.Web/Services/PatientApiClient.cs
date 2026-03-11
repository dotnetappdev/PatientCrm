using System.Net.Http.Headers;
using System.Net.Http.Json;
using PatientCrm.Core.Entities;

namespace PatientCrm.Web.Services;

/// <summary>Typed HTTP client that wraps all calls to the PatientCRM API.</summary>
public class PatientApiClient
{
    private readonly HttpClient _http;
    private readonly TokenProvider _tokenProvider;

    public PatientApiClient(HttpClient http, TokenProvider tokenProvider)
    {
        _http = http;
        _tokenProvider = tokenProvider;
    }

    // Attach the stored JWT to every outgoing request
    private void SetAuthHeader()
    {
        if (!string.IsNullOrEmpty(_tokenProvider.AccessToken))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
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

    public async Task<UserDetailDto?> GetUserAsync(Guid id)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<UserDetailDto>($"api/admin/users/{id}");
    }

    public async Task<bool> CreateUserAsync(object request)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/admin/users", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateUserAsync(Guid id, object request)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/admin/users/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleUserAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.PostAsync($"api/admin/users/{id}/toggle", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetUserPasswordAsync(Guid id, string newPassword)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/admin/users/{id}/reset-password", new { newPassword });
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool Ok, string? Error)> SetUserRoleAsync(Guid id, string role)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/admin/users/{id}/roles", new { role });
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, body);
    }

    // Appointments CRUD

    public async Task<Appointment?> CreateAppointmentAsync(Appointment appointment)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/appointments", appointment);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Appointment>();
    }

    public async Task<bool> UpdateAppointmentAsync(Guid id, Appointment appointment)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/appointments/{id}", appointment);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CancelAppointmentAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/appointments/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<VideoLinkResult?> GenerateVideoLinkAsync(Guid appointmentId, string platform, bool enablePasscode)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/appointments/{appointmentId}/generate-video-link", new { Platform = platform, EnablePasscode = enablePasscode });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VideoLinkResult>();
    }

    // Prescriptions CRUD

    public async Task<Prescription?> CreatePrescriptionAsync(Prescription prescription)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/prescriptions", prescription);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Prescription>();
    }

    public async Task<bool> UpdatePrescriptionAsync(Guid id, Prescription prescription)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/prescriptions/{id}", prescription);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePrescriptionAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/prescriptions/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool Ok, string? Error)> ReorderPrescriptionAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.PostAsync($"api/prescriptions/{id}/reorder", null);
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, body);
    }

    public async Task<bool> DiscontinuePrescriptionAsync(Guid id, Prescription prescription)
    {
        SetAuthHeader();
        prescription.Status = PatientCrm.Core.Enums.PrescriptionStatus.Cancelled;
        var response = await _http.PutAsJsonAsync($"api/prescriptions/{id}", prescription);
        return response.IsSuccessStatusCode;
    }

    // Image upload (multipart) – Blazor uses IBrowserFile
    public async Task<bool> UploadImageAsync(Guid patientId, Microsoft.AspNetCore.Components.Forms.IBrowserFile file, string title, string imageType, string? description, Guid? clinicalNoteId)
    {
        SetAuthHeader();
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(maxAllowedSize: 104_857_600);
        content.Add(new StreamContent(stream), "file", file.Name);
        content.Add(new StringContent(title), "title");
        content.Add(new StringContent(imageType), "imageType");
        if (!string.IsNullOrEmpty(description)) content.Add(new StringContent(description), "description");
        if (clinicalNoteId.HasValue) content.Add(new StringContent(clinicalNoteId.ToString()!), "clinicalNoteId");

        var response = await _http.PostAsync($"api/images?patientId={patientId}", content);
        return response.IsSuccessStatusCode;
    }

    // Departments

    public async Task<List<Department>?> GetDepartmentsAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<Department>>("api/departments");
    }

    public async Task<Department?> GetDepartmentAsync(Guid id)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<Department>($"api/departments/{id}");
    }

    public async Task<Department?> CreateDepartmentAsync(Department department)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/departments", department);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Department>();
    }

    public async Task<bool> UpdateDepartmentAsync(Guid id, Department department)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/departments/{id}", department);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteDepartmentAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/departments/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Ward>?> GetWardsAsync(Guid departmentId)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<Ward>>($"api/departments/{departmentId}/wards");
    }

    public async Task<Ward?> CreateWardAsync(Guid departmentId, Ward ward)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/departments/{departmentId}/wards", ward);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Ward>();
    }

    public async Task<bool> UpdateWardAsync(Guid wardId, Ward ward)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/departments/wards/{wardId}", ward);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteWardAsync(Guid wardId)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/departments/wards/{wardId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<PatientAdmission>?> GetPatientAdmissionsAsync(Guid? patientId = null, Guid? departmentId = null)
    {
        SetAuthHeader();
        var url = "api/departments/admissions?";
        if (patientId.HasValue) url += $"patientId={patientId}&";
        if (departmentId.HasValue) url += $"departmentId={departmentId}";
        return await _http.GetFromJsonAsync<List<PatientAdmission>>(url);
    }

    public async Task<PatientAdmission?> CreateAdmissionAsync(PatientAdmission admission)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/departments/admissions", admission);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PatientAdmission>();
    }

    public async Task<bool> UpdateAdmissionAsync(Guid id, PatientAdmission admission)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/departments/admissions/{id}", admission);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DischargePatientAsync(Guid admissionId, PatientAdmission admission)
    {
        SetAuthHeader();
        admission.Status = PatientCrm.Core.Enums.AdmissionStatus.Discharged;
        admission.DischargeDate ??= DateTime.UtcNow;
        var response = await _http.PutAsJsonAsync($"api/departments/admissions/{admissionId}", admission);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EnablePatientPortalAccessAsync(Guid patientId)
    {
        SetAuthHeader();
        var response = await _http.PostAsync($"api/patients/{patientId}/portal-access", null);
        return response.IsSuccessStatusCode;
    }

    // Letters

    public async Task<List<Letter>?> GetLettersAsync(Guid? patientId = null)
    {
        SetAuthHeader();
        var url = "api/letters";
        if (patientId.HasValue) url += $"?patientId={patientId}";
        return await _http.GetFromJsonAsync<List<Letter>>(url);
    }

    public async Task<Letter?> GetLetterAsync(Guid id)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<Letter>($"api/letters/{id}");
    }

    public async Task<Letter?> CreateLetterAsync(Letter letter)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/letters", letter);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Letter>();
    }

    public async Task<bool> UpdateLetterAsync(Guid id, Letter letter)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/letters/{id}", letter);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteLetterAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/letters/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendLetterAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.PostAsync($"api/letters/{id}/send", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<LetterTemplate>?> GetLetterTemplatesAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<LetterTemplate>>("api/letters/templates");
    }

    public async Task<LetterTemplate?> CreateLetterTemplateAsync(LetterTemplate template)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/letters/templates", template);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LetterTemplate>();
    }

    public async Task<bool> UpdateLetterTemplateAsync(Guid id, LetterTemplate template)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/letters/templates/{id}", template);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteLetterTemplateAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/letters/templates/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Letter>?> GetMyPortalLettersAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<Letter>>("api/portal/me/letters");
    }

    // Dental

    public async Task<DentalRecord?> GetDentalRecordAsync(Guid patientId)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<DentalRecord>($"api/dental/{patientId}");
    }

    public async Task<bool> UpdateDentalRecordAsync(Guid id, DentalRecord record)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/dental/{id}", record);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ToothRecord>?> GetToothRecordsAsync(Guid patientId)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ToothRecord>>($"api/dental/{patientId}/teeth");
    }

    public async Task<ToothRecord?> SaveToothRecordAsync(Guid patientId, ToothRecord tooth)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/dental/{patientId}/teeth", tooth);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ToothRecord>();
    }

    public async Task<bool> UpdateToothRecordAsync(Guid id, ToothRecord tooth)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/dental/tooth/{id}", tooth);
        return response.IsSuccessStatusCode;
    }

    // Patient portal — Patient role only

    public async Task<Patient?> GetMyPortalRecordAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<Patient>("api/portal/me");
    }
}

// DTOs for API responses

public record VideoLinkResult(string? Link, string? Passcode, string? Platform);

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

public class UserDetailDto : UserDto
{
    public string? NmcPin { get; set; }
    public List<string> Roles { get; set; } = [];
}
