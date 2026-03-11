namespace PatientCrm.Maui.Models;

public class Letter
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? TemplateId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorTitle { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? PracticeName { get; set; }
    public string? PracticeAddress { get; set; }
    public LetterStatus Status { get; set; }
    public DateTime LetterDate { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsPrinted { get; set; }
    public bool IsPatientVisible { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LetterTemplate
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public bool IsActive { get; set; }
}
