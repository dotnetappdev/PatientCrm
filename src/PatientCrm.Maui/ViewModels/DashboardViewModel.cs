using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private DashboardStats? _stats;

    public DashboardViewModel(ApiService api)
    {
        _api = api;
        Title = "Dashboard";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            Stats = await _api.GetDashboardStatsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Could not load dashboard: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
