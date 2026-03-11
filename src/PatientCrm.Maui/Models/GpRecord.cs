namespace PatientCrm.Maui.Models;

public class GpRecord
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string? PreviousGpPractice { get; set; }
    public bool IsTemporaryRegistration { get; set; }
    public bool HasDiabetes { get; set; }
    public bool HasHypertension { get; set; }
    public bool HasAsthma { get; set; }
    public bool HasCopd { get; set; }
    public bool HasHeartDisease { get; set; }
    public bool HasStrokeHistory { get; set; }
    public bool HasCancer { get; set; }
    public bool HasMentalHealthCondition { get; set; }
    public bool HasLearningDisability { get; set; }
    public decimal? Bmi { get; set; }
    public int? SystolicBloodPressure { get; set; }
    public int? DiastolicBloodPressure { get; set; }
    public decimal? Hba1c { get; set; }
    public decimal? Cholesterol { get; set; }
    public bool FluVaccination { get; set; }
    public DateTime? FluVaccinationDate { get; set; }
    public bool CovidVaccination { get; set; }
    public string? VaccinationHistory { get; set; }
}
