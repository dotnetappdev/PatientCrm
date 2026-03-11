using PatientCrm.Maui.ViewModels.Portal;

namespace PatientCrm.Maui.Views.Portal;

public partial class PortalPrescriptionsPage : ContentPage
{
    public PortalPrescriptionsPage(PortalPrescriptionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PortalPrescriptionsViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
