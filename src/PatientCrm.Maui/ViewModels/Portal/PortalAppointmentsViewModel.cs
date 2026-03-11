using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels.Portal;

public partial class PortalAppointmentsViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private ObservableCollection<Appointment> _upcomingAppointments = [];

    [ObservableProperty]
    private ObservableCollection<Appointment> _pastAppointments = [];

    public PortalAppointmentsViewModel(ApiService api)
    {
        _api = api;
        Title = "My Appointments";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var all = await _api.GetMyPortalAppointmentsAsync() ?? [];
            var now = DateTime.Now;
            UpcomingAppointments = new ObservableCollection<Appointment>(
                all.Where(a => a.StartTime >= now).OrderBy(a => a.StartTime));
            PastAppointments = new ObservableCollection<Appointment>(
                all.Where(a => a.StartTime < now).OrderByDescending(a => a.StartTime));
        }
        catch (Exception ex)
        {
            SetError($"Could not load appointments: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
