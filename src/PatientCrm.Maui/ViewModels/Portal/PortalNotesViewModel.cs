using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels.Portal;

public partial class PortalNotesViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private ObservableCollection<ClinicalNote> _notes = [];

    public PortalNotesViewModel(ApiService api)
    {
        _api = api;
        Title = "My Clinical Notes";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var list = await _api.GetMyPortalNotesAsync() ?? [];
            Notes = new ObservableCollection<ClinicalNote>(list.OrderByDescending(n => n.NoteDate));
        }
        catch (Exception ex)
        {
            SetError($"Could not load notes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
