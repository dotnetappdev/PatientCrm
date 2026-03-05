namespace PatientCrm.Core.Entities;

public class GpRecord : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    // Registration
    public DateTime? RegistrationDate { get; set; }
    public string? PreviousGpPractice { get; set; }
    public bool IsTemporaryRegistration { get; set; }

    // NHS Scores
    public string? QofData { get; set; }
    public string? FrailtyScore { get; set; }
    public int? DeprivationScore { get; set; }

    // Long term conditions
    public bool HasDiabetes { get; set; }
    public string? DiabetesType { get; set; }
    public bool HasHypertension { get; set; }
    public bool HasAsthma { get; set; }
    public bool HasCopd { get; set; }
    public bool HasHeartDisease { get; set; }
    public bool HasStrokeHistory { get; set; }
    public bool HasCancer { get; set; }
    public string? CancerDetails { get; set; }
    public bool HasMentalHealthCondition { get; set; }
    public string? MentalHealthDetails { get; set; }
    public bool HasLearningDisability { get; set; }
    public bool IsCarerForPatient { get; set; }
    public bool PatientIsACarer { get; set; }

    // Screening
    public DateTime? LastCervicalScreening { get; set; }
    public DateTime? LastBreastScreening { get; set; }
    public DateTime? LastBowelScreening { get; set; }
    public DateTime? LastAaaScan { get; set; }

    // Vaccinations
    public bool HasFluVaccination { get; set; }
    public DateTime? LastFluVaccinationDate { get; set; }
    public bool HasCovidVaccination { get; set; }
    public string? CovidVaccinationDetails { get; set; }
    public string? VaccinationHistory { get; set; }

    // Vitals baseline
    public decimal? BaselineBmi { get; set; }
    public decimal? BaselineBloodPressureSystolic { get; set; }
    public decimal? BaselineBloodPressureDiastolic { get; set; }
    public decimal? BaselineHba1c { get; set; }
    public decimal? BaselineCholesterol { get; set; }

    // Enhanced services
    public bool OnPalliativeCareRegister { get; set; }
    public bool OnDementiaRegister { get; set; }
    public bool OnEpilepsyRegister { get; set; }
    public bool OnAtrialFibrillationRegister { get; set; }

    public ICollection<Referral> Referrals { get; set; } = new List<Referral>();
}

public class Referral : BaseEntity
{
    public Guid GpRecordId { get; set; }
    public GpRecord? GpRecord { get; set; }

    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public required string Specialty { get; set; }
    public required string ReferralReason { get; set; }
    public string? HospitalName { get; set; }
    public string? ConsultantName { get; set; }
    public string? UbrNNumber { get; set; }
    public DateTime ReferralDate { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public string? Outcome { get; set; }
    public bool IsUrgent { get; set; }
    public string? Status { get; set; }
}
