using Microsoft.AspNetCore.Identity;

namespace PatientCrm.Core.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string? Title { get; set; }
    public string? GmcNumber { get; set; }
    public string? GdcNumber { get; set; }
    public string? NmcPin { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ProfilePictureUrl { get; set; }
    public string? Theme { get; set; } = "light";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public Guid? TenantId { get; set; }
}
