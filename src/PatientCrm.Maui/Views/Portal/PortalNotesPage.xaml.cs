using PatientCrm.Maui.ViewModels.Portal;

namespace PatientCrm.Maui.Views.Portal;

public partial class PortalNotesPage : ContentPage
{
    public PortalNotesPage(PortalNotesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PortalNotesViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
