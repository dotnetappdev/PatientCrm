using PatientCrm.Maui.ViewModels;

namespace PatientCrm.Maui.Views.Appointments;

public partial class AppointmentListPage : ContentPage
{
    public AppointmentListPage(AppointmentListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AppointmentListViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
