using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class PatientAdmission : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? WardId { get; set; }
    public Ward? Ward { get; set; }

    public Guid? ConsultantId { get; set; }
    public ApplicationUser? Consultant { get; set; }

    public string? BedNumber { get; set; }
    public AdmissionType AdmissionType { get; set; } = AdmissionType.Elective;
    public AdmissionStatus Status { get; set; } = AdmissionStatus.Active;

    public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;
    public DateTime? DischargeDate { get; set; }
    public DateTime? ExpectedDischargeDate { get; set; }

    public string? AdmissionReason { get; set; }
    public string? DiagnosisOnAdmission { get; set; }
    public string? DiagnosisOnDischarge { get; set; }
    public string? DischargeNotes { get; set; }

    public string? ReferringGpName { get; set; }
    public string? ReferringGpOdsCode { get; set; }

    public string? TriageNotes { get; set; }
    public string? TreatmentSummary { get; set; }
}
