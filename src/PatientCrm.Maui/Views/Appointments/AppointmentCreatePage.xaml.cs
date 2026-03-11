using PatientCrm.Maui.ViewModels;

namespace PatientCrm.Maui.Views.Appointments;

public partial class AppointmentCreatePage : ContentPage
{
    public AppointmentCreatePage(AppointmentCreateViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
