namespace PatientCrm.Maui.Models;

public class Prescription
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid PrescriberId { get; set; }
    public string? PrescriberName { get; set; }
    public string? MedicationName { get; set; }
    public string? GenericName { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Route { get; set; }
    public string? Instructions { get; set; }
    public int QuantityIssued { get; set; }
    public string? Unit { get; set; }
    public int Repeats { get; set; }
    public int RepeatsRemaining { get; set; }
    public DateTime PrescribedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PrescriptionStatus Status { get; set; }
    public string? SnomedCode { get; set; }
    public string? DmdCode { get; set; }
    public bool IsControlledDrug { get; set; }
    public string? Indication { get; set; }
    public string? SideEffectsNoted { get; set; }
    public DateTime CreatedAt { get; set; }
}
