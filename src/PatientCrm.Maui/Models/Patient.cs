namespace PatientCrm.Maui.Models;

public class Patient
{
    public Guid Id { get; set; }
    public string? NhsNumber { get; set; }
    public string? HscniNumber { get; set; }
    public string? ChiNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? PreferredName { get; set; }
    public string FullName => string.IsNullOrEmpty(PreferredName) ? $"{FirstName} {LastName}" : $"{PreferredName} ({FirstName} {LastName})";
    public DateTime DateOfBirth { get; set; }
    public int Age => (int)((DateTime.Today - DateOfBirth).TotalDays / 365.25);
    public Gender Gender { get; set; }
    public string? GenderIdentity { get; set; }
    public string? Ethnicity { get; set; }
    public BloodGroup BloodGroup { get; set; }
    public string? Religion { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool RequiresInterpreter { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string? Postcode { get; set; }
    public NhsRegion Region { get; set; }
    public string? Country { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? RegisteredGpName { get; set; }
    public string? RegisteredGpPracticeName { get; set; }
    public string? RegisteredGpOdsCode { get; set; }
    public PatientStatus Status { get; set; }
    public DateTime? DeceasedDate { get; set; }
    public string? Allergies { get; set; }
    public string? CurrentMedications { get; set; }
    public string? SmokingHistory { get; set; }
    public string? AlcoholHistory { get; set; }
    public string? MedicalAlerts { get; set; }
    public bool ConsentToTreatment { get; set; }
    public bool ConsentToDataSharing { get; set; }
    public bool ConsentToMarketing { get; set; }
    public DateTime? ConsentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ClinicalNote> ClinicalNotes { get; set; } = [];
    public List<Appointment> Appointments { get; set; } = [];
    public List<Prescription> Prescriptions { get; set; } = [];
    public List<PatientAlert> Alerts { get; set; } = [];
    public List<PatientAdmission> Admissions { get; set; } = [];
    public DentalRecord? DentalRecord { get; set; }
    public GpRecord? GpRecord { get; set; }
}
