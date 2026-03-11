namespace PatientCrm.Maui.Models;

public class PatientAlert
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public AlertSeverity Severity { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
