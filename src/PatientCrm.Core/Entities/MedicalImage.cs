using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class MedicalImage : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public Guid? ClinicalNoteId { get; set; }
    public ClinicalNote? ClinicalNote { get; set; }

    public ImageType ImageType { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? BodyPart { get; set; }

    // File storage
    public required string FileName { get; set; }
    public required string StoragePath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }

    // MRI specific
    public string? DicomSeriesId { get; set; }
    public string? DicomStudyId { get; set; }
    public int? SliceCount { get; set; }
    public string? Modality { get; set; }
    public string? EquipmentUsed { get; set; }
    public string? RadiologistName { get; set; }
    public string? RadiologistReport { get; set; }

    // Dental X-Ray specific
    public int? ToothNumber { get; set; }
    public string? TeethIncluded { get; set; }

    public DateTime ImageDate { get; set; } = DateTime.UtcNow;
    public Guid? UploadedById { get; set; }
    public ApplicationUser? UploadedBy { get; set; }
    public bool IsConfidential { get; set; }
    public string? Notes { get; set; }
}
