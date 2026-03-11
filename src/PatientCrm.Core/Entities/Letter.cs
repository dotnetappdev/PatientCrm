using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class Letter : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public Guid? TemplateId { get; set; }
    public LetterTemplate? Template { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorTitle { get; set; }

    public required string Subject { get; set; }
    public required string Body { get; set; }

    // Letterhead override (falls back to tenant settings if null)
    public string? PracticeName { get; set; }
    public string? PracticeAddress { get; set; }
    public string? PracticeLogoBase64 { get; set; }

    public LetterStatus Status { get; set; } = LetterStatus.Draft;
    public DateTime LetterDate { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public bool IsPrinted { get; set; }

    public bool IsPatientVisible { get; set; } = true;
}

public class LetterTemplate : BaseEntity
{
    public required string Title { get; set; }
    public string? Category { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public bool IsActive { get; set; } = true;
}
