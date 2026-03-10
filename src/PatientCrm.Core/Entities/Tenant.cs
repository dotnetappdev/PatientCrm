using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public TenantType TenantType { get; set; }
    public ClientType ClientType { get; set; }
    public string? OdsCode { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Postcode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? ContactName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.SqlServer;
    public string? ConnectionString { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
