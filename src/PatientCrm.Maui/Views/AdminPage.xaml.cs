using PatientCrm.Maui.ViewModels;

namespace PatientCrm.Maui.Views;

public partial class AdminPage : ContentPage
{
    public AdminPage(AdminViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
