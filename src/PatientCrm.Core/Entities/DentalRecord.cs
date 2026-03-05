namespace PatientCrm.Core.Entities;

public class DentalRecord : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    // Dental history
    public string? SmokingStatus { get; set; }
    public string? AlcoholConsumption { get; set; }
    public string? DietaryHabits { get; set; }
    public bool HasDentalAnxiety { get; set; }
    public string? AnxietyNotes { get; set; }

    // Oral health
    public DateTime? LastExaminationDate { get; set; }
    public DateTime? LastHygieneAppointment { get; set; }
    public DateTime? LastXRayDate { get; set; }
    public string? OralHygieneSummary { get; set; }
    public string? PeriodontalStatus { get; set; }
    public string? OrthodonticHistory { get; set; }

    // Prosthetics
    public bool HasDentures { get; set; }
    public string? DentureDetails { get; set; }
    public bool HasImplants { get; set; }
    public string? ImplantDetails { get; set; }
    public bool HasCrowns { get; set; }
    public bool HasBridges { get; set; }

    // NHS dental banding
    public string? NhsBandingHistory { get; set; }
    public string? FdiChartData { get; set; }
    public string? BasicPeriodontalExamination { get; set; }

    // Referrals
    public bool ReferredToOralSurgery { get; set; }
    public bool ReferredToOrthodontics { get; set; }
    public bool ReferredToPeriodontist { get; set; }

    public ICollection<ToothRecord> ToothRecords { get; set; } = new List<ToothRecord>();
}

public class ToothRecord : BaseEntity
{
    public Guid DentalRecordId { get; set; }
    public DentalRecord? DentalRecord { get; set; }

    public int ToothNumber { get; set; }
    public string? ToothName { get; set; }
    public bool IsPrimary { get; set; }

    // Status flags – all possible dental defects
    public bool IsMissing { get; set; }
    public bool IsExtracted { get; set; }
    public bool HasDecay { get; set; }
    public bool HasFilling { get; set; }
    public string? FillingType { get; set; }   // Amalgam, Composite, GlassIonomer, Gold
    public bool HasCrown { get; set; }
    public string? CrownMaterial { get; set; } // PFM, ZirconiaAll-Ceramic, Metal
    public bool HasBridge { get; set; }
    public bool HasVeneer { get; set; }
    public bool HasImplant { get; set; }
    public bool HasRootCanal { get; set; }
    public bool HasAbscess { get; set; }
    public bool HasFracture { get; set; }
    public bool HasSensitivity { get; set; }
    public bool HasGumDisease { get; set; }
    public bool HasChipping { get; set; }
    public bool HasWearing { get; set; }
    public bool HasOverhang { get; set; }
    public bool IsWatchAndWait { get; set; }

    // Surfaces affected (comma-separated: M,D,B,L,O,P,I)
    public string? Surfaces { get; set; }

    public string? Notes { get; set; }
    public string? TreatmentPlan { get; set; }
    public DateTime? TreatmentDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
}
