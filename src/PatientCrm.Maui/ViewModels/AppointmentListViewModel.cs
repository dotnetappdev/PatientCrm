using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels;

public partial class AppointmentListViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private ObservableCollection<Appointment> _appointments = [];

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    public AppointmentListViewModel(ApiService api)
    {
        _api = api;
        Title = "Appointments";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var list = await _api.GetAppointmentsAsync(SelectedDate);
            Appointments = new ObservableCollection<Appointment>(list ?? []);
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

    [RelayCommand]
    private async Task PreviousDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(1);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task TodayAsync()
    {
        SelectedDate = DateTime.Today;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task BookAppointmentAsync()
    {
        await Shell.Current.GoToAsync("appointment-create");
    }
}
