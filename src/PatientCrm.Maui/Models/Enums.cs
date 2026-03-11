namespace PatientCrm.Maui.Models;

public enum Gender { Male, Female, NonBinary, PreferNotToSay, Other }
public enum PatientStatus { Active, Inactive, Deceased, Transferred }
public enum NhsRegion { England, Wales, Scotland, NorthernIreland }
public enum DisciplineType { GP, Dentist, Consultant, Specialist, Physiotherapist, MentalHealth, Nurse }
public enum NoteType { Consultation, Referral, FollowUp, Prescription, LabResult, DentalExamination, DentalTreatment, DentalXRay, ImagingReport, MriReport, SpecialistReport, Discharge, Triage, HomeVisit, TelephoneConsultation }
public enum AppointmentStatus { Scheduled, Confirmed, InProgress, Completed, Cancelled, DidNotAttend, Rescheduled }
public enum AppointmentType { InPerson, Telephone, VideoCall, HomeVisit }
public enum PrescriptionStatus { Active, Completed, Cancelled, OnHold }
public enum ImageType { Photograph, XRay, MriScan, CtScan, Ultrasound, Ecg, DentalXRay, Panoramic, Periapical, Bitewing, Other }
public enum BloodGroup { APositive, ANegative, BPositive, BNegative, ABPositive, ABNegative, OPositive, ONegative, Unknown }
public enum TenantType { NhsEngland, NhsWales, NhsScotland, Hscni, PrivatePractice }
public enum ClientType { GpPractice, DentalPractice, HospitalConsulting, HealthCentre, WalkInCentre, MentalHealthService, PhysiotherapyPractice }
public enum AdmissionType { Emergency, Elective, DayCase, OutPatient, Transfer }
public enum AdmissionStatus { Active, Discharged, OnLeave, Transferred, Deceased }
public enum LetterStatus { Draft, Final, Sent, Archived }
public enum AlertSeverity { Low, Medium, High, Critical }
public enum DepartmentType { GeneralMedicine, Cardiology, Neurology, Oncology, Orthopaedics, Paediatrics, Obstetrics, Gynaecology, Psychiatry, Dermatology, Gastroenterology, Ophthalmology, ENT, Radiology, Pathology, Pharmacy, Physiotherapy, Occupational, SpeechTherapy, DietAndNutrition, SocialWork, Dental, OralSurgery, MaxillofacialSurgery, PlasticSurgery, GeneralSurgery }
