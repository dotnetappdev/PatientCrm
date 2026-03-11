using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Models;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels;

public partial class PatientCreateViewModel : BaseViewModel
{
    private readonly ApiService _api;

    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private DateTime _dateOfBirth = DateTime.Today.AddYears(-30);
    [ObservableProperty] private string _nhsNumber = string.Empty;
    [ObservableProperty] private string _hscniNumber = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _mobileNumber = string.Empty;
    [ObservableProperty] private string _addressLine1 = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _postcode = string.Empty;
    [ObservableProperty] private string _allergies = string.Empty;
    [ObservableProperty] private string _currentMedications = string.Empty;
    [ObservableProperty] private string _emergencyContactName = string.Empty;
    [ObservableProperty] private string _emergencyContactRelationship = string.Empty;
    [ObservableProperty] private string _emergencyContactPhone = string.Empty;
    [ObservableProperty] private string _registeredGpName = string.Empty;
    [ObservableProperty] private string _registeredGpPracticeName = string.Empty;
    [ObservableProperty] private string _registeredGpOdsCode = string.Empty;
    [ObservableProperty] private Gender _gender = Gender.PreferNotToSay;
    [ObservableProperty] private BloodGroup _bloodGroup = BloodGroup.Unknown;
    [ObservableProperty] private NhsRegion _region = NhsRegion.England;

    public PatientCreateViewModel(ApiService api)
    {
        _api = api;
        Title = "Register Patient";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            SetError("First name and last name are required.");
            return;
        }

        IsBusy = true;
        ClearMessages();
        try
        {
            var patient = new Patient
            {
                FirstName = FirstName,
                LastName = LastName,
                DateOfBirth = DateOfBirth,
                NhsNumber = NhsNumber,
                HscniNumber = HscniNumber,
                Email = Email,
                PhoneNumber = PhoneNumber,
                MobileNumber = MobileNumber,
                AddressLine1 = AddressLine1,
                City = City,
                Postcode = Postcode,
                Allergies = Allergies,
                CurrentMedications = CurrentMedications,
                EmergencyContactName = EmergencyContactName,
                EmergencyContactRelationship = EmergencyContactRelationship,
                EmergencyContactPhone = EmergencyContactPhone,
                RegisteredGpName = RegisteredGpName,
                RegisteredGpPracticeName = RegisteredGpPracticeName,
                RegisteredGpOdsCode = RegisteredGpOdsCode,
                Gender = Gender,
                BloodGroup = BloodGroup,
                Region = Region,
                Status = PatientStatus.Active,
                ConsentToTreatment = true,
                ConsentToDataSharing = true
            };
            var result = await _api.CreatePatientAsync(patient);
            if (result != null)
            {
                await Shell.Current.GoToAsync($"//patients/detail?id={result.Id}");
            }
            else
            {
                SetError("Could not create patient. Please try again.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
