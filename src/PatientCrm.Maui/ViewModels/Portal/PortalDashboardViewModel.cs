using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels.Portal;

public partial class PortalDashboardViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private Patient? _patientRecord;

    [ObservableProperty]
    private ObservableCollection<Appointment> _upcomingAppointments = [];

    [ObservableProperty]
    private ObservableCollection<Prescription> _activePrescriptions = [];

    public PortalDashboardViewModel(ApiService api)
    {
        _api = api;
        Title = "My Health";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            PatientRecord = await _api.GetMyPortalRecordAsync();
            var appointments = await _api.GetMyPortalAppointmentsAsync();
            var prescriptions = await _api.GetMyPortalPrescriptionsAsync();

            UpcomingAppointments = new ObservableCollection<Appointment>(
                (appointments ?? []).Where(a => a.StartTime >= DateTime.Now)
                    .OrderBy(a => a.StartTime).Take(4));

            ActivePrescriptions = new ObservableCollection<Prescription>(
                (prescriptions ?? []).Where(p => p.Status == PrescriptionStatus.Active).Take(5));
        }
        catch (Exception ex)
        {
            SetError($"Could not load your health record: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
