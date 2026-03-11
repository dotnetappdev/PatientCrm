using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels.Portal;

public partial class PortalPrescriptionsViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty]
    private ObservableCollection<Prescription> _prescriptions = [];

    public PortalPrescriptionsViewModel(ApiService api)
    {
        _api = api;
        Title = "My Medications";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var list = await _api.GetMyPortalPrescriptionsAsync() ?? [];
            Prescriptions = new ObservableCollection<Prescription>(list.OrderByDescending(p => p.PrescribedDate));
        }
        catch (Exception ex)
        {
            SetError($"Could not load medications: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RequestRepeatAsync(Prescription prescription)
    {
        if (prescription.RepeatsRemaining <= 0)
        {
            SetError("No repeats remaining. Please contact your practice.");
            return;
        }
        var (ok, err) = await _api.ReorderPrescriptionAsync(prescription.Id);
        if (ok) SetSuccess($"Repeat requested for {prescription.MedicationName}.");
        else SetError(err ?? "Could not request repeat.");
    }
}
