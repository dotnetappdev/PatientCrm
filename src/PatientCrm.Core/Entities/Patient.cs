using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class Patient : BaseEntity
{
    // NHS / HSCNI identifiers
    public string? NhsNumber { get; set; }
    public string? HscniNumber { get; set; }
    public string? ChiNumber { get; set; }

    // Demographics
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? PreferredName { get; set; }
    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {MiddleName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? GenderIdentity { get; set; }
    public string? Ethnicity { get; set; }
    public BloodGroup BloodGroup { get; set; }
    public string? Religion { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool RequiresInterpreter { get; set; }

    // Contact
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string? Postcode { get; set; }
    public NhsRegion Region { get; set; }
    public string? Country { get; set; } = "United Kingdom";

    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // GP
    public string? RegisteredGpName { get; set; }
    public string? RegisteredGpPractice { get; set; }
    public string? RegisteredGpOdsCode { get; set; }

    // Status
    public PatientStatus Status { get; set; } = PatientStatus.Active;
    public DateTime? DeceasedDate { get; set; }

    // Clinical
    public string? Allergies { get; set; }
    public string? CurrentMedications { get; set; }
    public bool HasSmokingHistory { get; set; }
    public bool HasAlcoholHistory { get; set; }
    public string? MedicalAlerts { get; set; }

    // Consent
    public bool ConsentToTreatment { get; set; }
    public bool ConsentToDataSharing { get; set; }
    public bool ConsentToMarketing { get; set; }
    public DateTime? ConsentDate { get; set; }

    // Navigation
    public ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<MedicalImage> MedicalImages { get; set; } = new List<MedicalImage>();
    public ICollection<PatientAlert> Alerts { get; set; } = new List<PatientAlert>();
    public DentalRecord? DentalRecord { get; set; }
    public GpRecord? GpRecord { get; set; }
}
