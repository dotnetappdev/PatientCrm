using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels.Portal;

public partial class PortalProfileViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private Patient? _patient;

    public PortalProfileViewModel(ApiService api)
    {
        _api = api;
        Title = "My Profile";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            Patient = await _api.GetMyPortalRecordAsync();
        }
        catch (Exception ex)
        {
            SetError($"Could not load profile: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
