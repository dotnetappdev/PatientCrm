using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class Appointment : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public Guid ProviderId { get; set; }
    public ApplicationUser? Provider { get; set; }

    public AppointmentType AppointmentType { get; set; }
    public DisciplineType Discipline { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public required string Title { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;

    public string? Location { get; set; }
    public string? Room { get; set; }
    public string? VideoCallLink { get; set; }
    public string? VideoCallPasscode { get; set; }
    public string? VideoCallPlatform { get; set; } // "GoogleMeet", "Zoom", "MicrosoftTeams", "AccuBook"

    public bool IsUrgent { get; set; }
    public bool PatientNotified { get; set; }
    public DateTime? ReminderSentAt { get; set; }

    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public Guid? RescheduledFromId { get; set; }
}
