using PatientCrm.Maui.ViewModels.Portal;

namespace PatientCrm.Maui.Views.Portal;

public partial class PortalLettersPage : ContentPage
{
    public PortalLettersPage(PortalLettersViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PortalLettersViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
