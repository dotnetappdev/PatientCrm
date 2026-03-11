namespace PatientCrm.Maui.Models;

public class ClinicalNote
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public NoteType NoteType { get; set; }
    public DisciplineType DisciplineType { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? SnomedCode { get; set; }
    public string? IcdCode { get; set; }
    public string? ReadCode { get; set; }
    public bool IsConfidential { get; set; }
    public bool IsLocked { get; set; }
    public bool RequiresFollowUp { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public Guid AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public DateTime NoteDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
