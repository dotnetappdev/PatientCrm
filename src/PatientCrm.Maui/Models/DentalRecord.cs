namespace PatientCrm.Maui.Models;

public class DentalRecord
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string? SmokingStatus { get; set; }
    public string? AlcoholConsumption { get; set; }
    public string? DietaryHabits { get; set; }
    public bool HasDentalAnxiety { get; set; }
    public string? AnxietyNotes { get; set; }
    public DateTime? LastExaminationDate { get; set; }
    public DateTime? LastHygieneAppointment { get; set; }
    public DateTime? LastXRayDate { get; set; }
    public string? OralHygieneSummary { get; set; }
    public string? PeriodontalStatus { get; set; }
    public string? OrthodonticHistory { get; set; }
    public bool HasDentures { get; set; }
    public bool HasImplants { get; set; }
    public bool HasCrowns { get; set; }
    public bool HasBridges { get; set; }
    public string? NhsBandingHistory { get; set; }
    public string? BasicPeriodontalExamination { get; set; }
    public bool ReferredToOralSurgery { get; set; }
    public bool ReferredToOrthodontics { get; set; }
    public bool ReferredToPeriodontist { get; set; }
    public List<ToothRecord> ToothRecords { get; set; } = [];
}

public class ToothRecord
{
    public Guid Id { get; set; }
    public Guid DentalRecordId { get; set; }
    public int ToothNumber { get; set; }
    public string? ToothName { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsMissing { get; set; }
    public bool IsExtracted { get; set; }
    public bool HasDecay { get; set; }
    public bool HasFilling { get; set; }
    public string? FillingType { get; set; }
    public bool HasCrown { get; set; }
    public string? CrownMaterial { get; set; }
    public bool HasBridge { get; set; }
    public bool HasImplant { get; set; }
    public bool HasRootCanal { get; set; }
    public bool HasAbscess { get; set; }
    public bool IsWatchAndWait { get; set; }
    public string? Notes { get; set; }
    public string? TreatmentPlan { get; set; }
    public DateTime? TreatmentDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
}
