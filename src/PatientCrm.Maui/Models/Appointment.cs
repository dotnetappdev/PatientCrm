namespace PatientCrm.Maui.Models;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public AppointmentType AppointmentType { get; set; }
    public DisciplineType DisciplineType { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Title { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? Location { get; set; }
    public string? Room { get; set; }
    public string? VideoCallLink { get; set; }
    public string? VideoCallPasscode { get; set; }
    public string? VideoCallPlatform { get; set; }
    public bool IsUrgent { get; set; }
    public bool PatientNotified { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
