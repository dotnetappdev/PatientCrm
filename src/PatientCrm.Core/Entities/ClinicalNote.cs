using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class ClinicalNote : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public NoteType NoteType { get; set; }
    public DisciplineType Discipline { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public string? Summary { get; set; }

    // SOAP format support
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }

    // Coding
    public string? SnomedCode { get; set; }
    public string? IcdCode { get; set; }
    public string? ReadCode { get; set; }

    // Flags
    public bool IsConfidential { get; set; }
    public bool IsLocked { get; set; }
    public bool RequiresFollowUp { get; set; }
    public DateTime? FollowUpDate { get; set; }

    // Author
    public Guid AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }
    public string? AuthorName { get; set; }
    public DateTime NoteDate { get; set; } = DateTime.UtcNow;

    // Attachments
    public ICollection<MedicalImage> Images { get; set; } = new List<MedicalImage>();
}
