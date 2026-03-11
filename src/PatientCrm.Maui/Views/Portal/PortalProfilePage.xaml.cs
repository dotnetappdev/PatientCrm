using PatientCrm.Maui.ViewModels.Portal;

namespace PatientCrm.Maui.Views.Portal;

public partial class PortalProfilePage : ContentPage
{
    public PortalProfilePage(PortalProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PortalProfileViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
