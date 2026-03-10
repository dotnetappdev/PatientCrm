using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class PatientAlert : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public required string Title { get; set; }
    public required string Description { get; set; }
    public AlertSeverity Severity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiryDate { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
}

public class AuditLog : BaseEntity
{
    public required string EntityName { get; set; }
    public Guid EntityId { get; set; }
    public required string Action { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
}
