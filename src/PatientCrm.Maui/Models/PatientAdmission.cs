namespace PatientCrm.Maui.Models;

public class PatientAdmission
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? WardId { get; set; }
    public string? WardName { get; set; }
    public Guid? ConsultantId { get; set; }
    public string? ConsultantName { get; set; }
    public string? BedNumber { get; set; }
    public AdmissionType AdmissionType { get; set; }
    public AdmissionStatus Status { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public DateTime? ExpectedDischargeDate { get; set; }
    public string? AdmissionReason { get; set; }
    public string? DiagnosisOnAdmission { get; set; }
    public string? DiagnosisOnDischarge { get; set; }
    public string? DischargeNotes { get; set; }
    public string? ReferringGpName { get; set; }
    public string? TriageNotes { get; set; }
    public string? TreatmentSummary { get; set; }
    public DateTime CreatedAt { get; set; }
}
