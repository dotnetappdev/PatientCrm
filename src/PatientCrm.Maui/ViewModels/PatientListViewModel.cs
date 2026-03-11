using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels;

public partial class PatientListViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private ObservableCollection<Patient> _patients = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalCount;

    private const int PageSize = 20;

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage * PageSize < TotalCount;

    public PatientListViewModel(ApiService api)
    {
        _api = api;
        Title = "Patients";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var result = await _api.GetPatientsAsync(SearchQuery, CurrentPage, PageSize);
            if (result != null)
            {
                Patients = new ObservableCollection<Patient>(result.Patients);
                TotalCount = result.Total;
            }
        }
        catch (Exception ex)
        {
            SetError($"Could not load patients: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (HasNextPage)
        {
            CurrentPage++;
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task ViewPatientAsync(Patient patient)
    {
        await Shell.Current.GoToAsync($"patient-detail?id={patient.Id}");
    }

    [RelayCommand]
    private async Task CreatePatientAsync()
    {
        await Shell.Current.GoToAsync("patient-create");
    }
}
