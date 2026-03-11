using PatientCrm.Maui.ViewModels.Portal;

namespace PatientCrm.Maui.Views.Portal;

public partial class PortalDashboardPage : ContentPage
{
    public PortalDashboardPage(PortalDashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PortalDashboardViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
