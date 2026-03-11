using PatientCrm.Maui.Views;

namespace PatientCrm.Maui;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        // Navigate to login on startup
        MainPage = new AppShell();
    }
}
