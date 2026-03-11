using PatientCrm.Maui.ViewModels.Portal;

namespace PatientCrm.Maui.Views.Portal;

public partial class PortalAppointmentsPage : ContentPage
{
    public PortalAppointmentsPage(PortalAppointmentsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PortalAppointmentsViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
