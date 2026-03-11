using PatientCrm.Maui.ViewModels;

namespace PatientCrm.Maui.Views.Patients;

public partial class PatientDetailPage : ContentPage
{
    public PatientDetailPage(PatientDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
