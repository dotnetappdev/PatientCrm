using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels;

public partial class DepartmentListViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private ObservableCollection<Department> _departments = [];

    [ObservableProperty]
    private Department? _selectedDepartment;

    public DepartmentListViewModel(ApiService api)
    {
        _api = api;
        Title = "Departments";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var list = await _api.GetDepartmentsAsync() ?? [];
            Departments = new ObservableCollection<Department>(list.OrderBy(d => d.Name));
        }
        catch (Exception ex)
        {
            SetError($"Could not load departments: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectDepartment(Department department)
    {
        SelectedDepartment = department;
    }
}
