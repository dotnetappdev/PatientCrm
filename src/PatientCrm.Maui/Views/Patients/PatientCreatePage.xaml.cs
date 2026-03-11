using PatientCrm.Maui.ViewModels;

namespace PatientCrm.Maui.Views.Patients;

public partial class PatientCreatePage : ContentPage
{
    public PatientCreatePage(PatientCreateViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
