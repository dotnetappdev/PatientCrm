using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels.Portal;

public partial class PortalLettersViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private ObservableCollection<Letter> _letters = [];

    [ObservableProperty]
    private Letter? _selectedLetter;

    public PortalLettersViewModel(ApiService api)
    {
        _api = api;
        Title = "My Letters";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var list = await _api.GetMyPortalLettersAsync() ?? [];
            Letters = new ObservableCollection<Letter>(list.OrderByDescending(l => l.LetterDate));
        }
        catch (Exception ex)
        {
            SetError($"Could not load letters: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ViewLetter(Letter letter)
    {
        SelectedLetter = letter;
    }

    [RelayCommand]
    private void CloseLetterViewer()
    {
        SelectedLetter = null;
    }
}
