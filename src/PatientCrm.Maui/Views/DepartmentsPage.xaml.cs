using PatientCrm.Maui.ViewModels;

namespace PatientCrm.Maui.Views;

public partial class DepartmentsPage : ContentPage
{
    public DepartmentsPage(DepartmentListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DepartmentListViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
