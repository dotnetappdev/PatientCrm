using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels;

public partial class AppointmentCreateViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty] private string _title2 = string.Empty;
    [ObservableProperty] private AppointmentType _appointmentType = AppointmentType.InPerson;
    [ObservableProperty] private DisciplineType _disciplineType = DisciplineType.GP;
    [ObservableProperty] private AppointmentStatus _status = AppointmentStatus.Scheduled;
    [ObservableProperty] private DateTime _date = DateTime.Today;
    [ObservableProperty] private TimeSpan _startTime = new(9, 0, 0);
    [ObservableProperty] private int _durationMinutes = 15;
    [ObservableProperty] private string _reason = string.Empty;
    [ObservableProperty] private string _location = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private bool _isUrgent;
    [ObservableProperty] private bool _patientNotified;
    [ObservableProperty] private string? _patientIdStr;

    public AppointmentCreateViewModel(ApiService api)
    {
        _api = api;
        Title = "Book Appointment";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title2))
        {
            SetError("Please enter an appointment title.");
            return;
        }

        IsBusy = true;
        ClearMessages();
        try
        {
            var start = Date.Date.Add(StartTime);
            var appointment = new Appointment
            {
                Title = Title2,
                AppointmentType = AppointmentType,
                DisciplineType = DisciplineType,
                Status = Status,
                StartTime = start,
                EndTime = start.AddMinutes(DurationMinutes),
                DurationMinutes = DurationMinutes,
                Reason = Reason,
                Location = Location,
                Notes = Notes,
                IsUrgent = IsUrgent,
                PatientNotified = PatientNotified
            };
            if (!string.IsNullOrWhiteSpace(PatientIdStr) && Guid.TryParse(PatientIdStr, out var pid))
                appointment.PatientId = pid;

            var result = await _api.CreateAppointmentAsync(appointment);
            if (result != null)
                await Shell.Current.GoToAsync("..");
            else
                SetError("Could not create appointment.");
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");
}
