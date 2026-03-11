namespace PatientCrm.Maui.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

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
    public List<string> Roles { get; set; } = [];
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
    public ClientType ClientType { get; set; }
    public TenantType TenantType { get; set; }
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
    public List<string> Roles { get; set; } = [];
}

public class VideoLinkResult
{
    public string? Link { get; set; }
    public string? Passcode { get; set; }
    public string? Platform { get; set; }
}
