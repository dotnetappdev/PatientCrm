using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels;

public partial class AdminViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private AdminStats? _stats;

    [ObservableProperty]
    private ObservableCollection<TenantSummary> _tenants = [];

    [ObservableProperty]
    private ObservableCollection<UserDto> _users = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public AdminViewModel(ApiService api)
    {
        _api = api;
        Title = "Administration";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            Stats = await _api.GetAdminStatsAsync();
            var tenants = await _api.GetTenantsAsync();
            Tenants = new ObservableCollection<TenantSummary>(tenants ?? []);
            var users = await _api.GetUsersAsync();
            Users = new ObservableCollection<UserDto>(users ?? []);
        }
        catch (Exception ex)
        {
            SetError($"Could not load admin data: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleTenantAsync(TenantSummary tenant)
    {
        var ok = await _api.ToggleTenantAsync(tenant.Id);
        if (ok)
        {
            tenant.IsActive = !tenant.IsActive;
            SetSuccess($"Tenant {tenant.Name} updated.");
        }
    }

    [RelayCommand]
    private async Task ToggleUserAsync(UserDto user)
    {
        var ok = await _api.ToggleUserAsync(user.Id);
        if (ok)
        {
            user.IsActive = !user.IsActive;
            SetSuccess($"User {user.FullName} updated.");
        }
    }
}
