using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class Prescription : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public Guid PrescriberId { get; set; }
    public ApplicationUser? Prescriber { get; set; }

    public required string MedicationName { get; set; }
    public string? GenericName { get; set; }
    public required string Dosage { get; set; }
    public required string Frequency { get; set; }
    public required string Route { get; set; }
    public string? Instructions { get; set; }
    public int? QuantityIssued { get; set; }
    public string? Unit { get; set; }
    public int? Repeats { get; set; }
    public int? RepeatsRemaining { get; set; }

    public DateTime PrescribedDate { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;

    public string? SnomedCode { get; set; }
    public string? DmdCode { get; set; }
    public bool IsControlledDrug { get; set; }
    public string? Indication { get; set; }
    public string? SideEffectsNoted { get; set; }
}
