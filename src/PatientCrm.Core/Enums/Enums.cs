namespace PatientCrm.Core.Enums;

public enum DatabaseProvider
{
    SqlServer,
    PostgreSql,
    MySql,
    Sqlite
}

public enum Gender
{
    Male,
    Female,
    NonBinary,
    PreferNotToSay,
    Other
}

public enum PatientStatus
{
    Active,
    Inactive,
    Deceased,
    Transferred
}

public enum NhsRegion
{
    England,
    Wales,
    Scotland,
    NorthernIreland
}

public enum DisciplineType
{
    GP,
    Dentist,
    Consultant,
    Specialist,
    Physiotherapist,
    MentalHealth,
    Nurse
}

public enum NoteType
{
    Consultation,
    Referral,
    FollowUp,
    Prescription,
    LabResult,
    DentalExamination,
    DentalTreatment,
    DentalXRay,
    ImagingReport,
    MriReport,
    SpecialistReport,
    Discharge,
    Triage,
    HomeVisit,
    TelephoneConsultation
}

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    DidNotAttend,
    Rescheduled
}

public enum AppointmentType
{
    InPerson,
    Telephone,
    VideoCall,
    HomeVisit
}

public enum PrescriptionStatus
{
    Active,
    Completed,
    Cancelled,
    OnHold
}

public enum ImageType
{
    Photograph,
    XRay,
    MriScan,
    CtScan,
    Ultrasound,
    Ecg,
    DentalXRay,
    Panoramic,
    Periapical,
    Bitewing,
    Other
}

public enum ToothSurface
{
    Mesial,
    Distal,
    Buccal,
    Lingual,
    Occlusal,
    Palatal,
    Incisal
}

public enum BloodGroup
{
    APositive,
    ANegative,
    BPositive,
    BNegative,
    ABPositive,
    ABNegative,
    OPositive,
    ONegative,
    Unknown
}

public enum TenantType
{
    NhsEngland,
    NhsWales,
    NhsScotland,
    Hscni,
    PrivatePractice
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum ClientType
{
    GpPractice,
    DentalPractice,
    HospitalConsulting,
    HealthCentre,
    WalkInCentre,
    MentalHealthService,
    PhysiotherapyPractice
}

public enum DepartmentType
{
    Cardiology,
    Neurology,
    Oncology,
    Orthopaedics,
    Emergency,
    Maternity,
    Psychiatry,
    Renal,
    Respiratory,
    Gastroenterology,
    Haematology,
    Dermatology,
    Rheumatology,
    Endocrinology,
    GeneralMedicine,
    GeneralSurgery,
    Urology,
    Ophthalmology,
    EarNoseThroat,
    Paediatrics,
    IntensiveCare,
    Radiology,
    Pathology,
    PhysiotherapyRehab,
    MaxillofacialSurgery
}

public enum AdmissionType
{
    Emergency,
    Elective,
    DayCase,
    OutPatient,
    Transfer
}

public enum AdmissionStatus
{
    Active,
    Discharged,
    OnLeave,
    Transferred,
    Deceased
}


public enum LetterStatus
{
    Draft,
    Final,
    Sent,
    Archived
}
