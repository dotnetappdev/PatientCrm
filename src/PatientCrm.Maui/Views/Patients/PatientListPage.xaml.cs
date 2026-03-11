using PatientCrm.Maui.ViewModels;

namespace PatientCrm.Maui.Views.Patients;

public partial class PatientListPage : ContentPage
{
    public PatientListPage(PatientListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PatientListViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
