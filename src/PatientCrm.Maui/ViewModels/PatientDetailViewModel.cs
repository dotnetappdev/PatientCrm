using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels;

[QueryProperty(nameof(PatientId), "id")]
public partial class PatientDetailViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private string? _patientId;

    [ObservableProperty]
    private Patient? _patient;

    [ObservableProperty]
    private ObservableCollection<ClinicalNote> _notes = [];

    [ObservableProperty]
    private ObservableCollection<Appointment> _appointments = [];

    [ObservableProperty]
    private ObservableCollection<Prescription> _prescriptions = [];

    [ObservableProperty]
    private ObservableCollection<PatientAdmission> _admissions = [];

    // New Note form
    [ObservableProperty]
    private string _newNoteTitle = string.Empty;
    [ObservableProperty]
    private string _newNoteContent = string.Empty;
    [ObservableProperty]
    private bool _newNoteIsConfidential;

    // New Prescription form
    [ObservableProperty]
    private string _newMedicationName = string.Empty;
    [ObservableProperty]
    private string _newDosage = string.Empty;
    [ObservableProperty]
    private string _newFrequency = string.Empty;

    public PatientDetailViewModel(ApiService api)
    {
        _api = api;
        Title = "Patient Record";
    }

    partial void OnPatientIdChanged(string? value)
    {
        if (value != null)
            _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (PatientId == null) return;
        if (!Guid.TryParse(PatientId, out var id)) return;

        IsBusy = true;
        ClearMessages();
        try
        {
            Patient = await _api.GetPatientAsync(id);
            if (Patient != null)
            {
                Title = Patient.FullName;
                Notes = new ObservableCollection<ClinicalNote>(Patient.ClinicalNotes);
                Appointments = new ObservableCollection<Appointment>(Patient.Appointments);
                Prescriptions = new ObservableCollection<Prescription>(Patient.Prescriptions);
                Admissions = new ObservableCollection<PatientAdmission>(Patient.Admissions);
            }
        }
        catch (Exception ex)
        {
            SetError($"Could not load patient: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditPatientAsync()
    {
        await Shell.Current.GoToAsync($"patient-edit?id={PatientId}");
    }

    [RelayCommand]
    private async Task AddNoteAsync()
    {
        if (string.IsNullOrWhiteSpace(NewNoteTitle) || PatientId == null) return;
        if (!Guid.TryParse(PatientId, out var id)) return;

        IsBusy = true;
        try
        {
            var note = new ClinicalNote
            {
                PatientId = id,
                Title = NewNoteTitle,
                Content = NewNoteContent,
                IsConfidential = NewNoteIsConfidential,
                NoteType = NoteType.Consultation,
                NoteDate = DateTime.UtcNow
            };
            var ok = await _api.AddNoteAsync(id, note);
            if (ok)
            {
                Notes.Insert(0, note);
                NewNoteTitle = string.Empty;
                NewNoteContent = string.Empty;
                NewNoteIsConfidential = false;
                SetSuccess("Clinical note added.");
            }
            else
            {
                SetError("Could not add note.");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddPrescriptionAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMedicationName) || PatientId == null) return;
        if (!Guid.TryParse(PatientId, out var id)) return;

        IsBusy = true;
        try
        {
            var prescription = new Prescription
            {
                PatientId = id,
                MedicationName = NewMedicationName,
                Dosage = NewDosage,
                Frequency = NewFrequency,
                PrescribedDate = DateTime.UtcNow,
                Status = PrescriptionStatus.Active
            };
            var result = await _api.CreatePrescriptionAsync(prescription);
            if (result != null)
            {
                Prescriptions.Insert(0, result);
                NewMedicationName = string.Empty;
                NewDosage = string.Empty;
                NewFrequency = string.Empty;
                SetSuccess("Prescription added.");
            }
            else
            {
                SetError("Could not add prescription.");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EnablePortalAccessAsync()
    {
        if (PatientId == null || !Guid.TryParse(PatientId, out var id)) return;
        var ok = await _api.EnablePatientPortalAccessAsync(id);
        if (ok) SetSuccess("Portal access enabled.");
        else SetError("Could not enable portal access.");
    }
}
