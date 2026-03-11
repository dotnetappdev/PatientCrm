using PatientCrm.Maui.Views;
using PatientCrm.Maui.Views.Patients;
using PatientCrm.Maui.Views.Appointments;

namespace PatientCrm.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("patient-detail", typeof(PatientDetailPage));
        Routing.RegisterRoute("patient-create", typeof(PatientCreatePage));
        Routing.RegisterRoute("patient-edit", typeof(PatientCreatePage)); // reuse create page for edit
        Routing.RegisterRoute("appointment-create", typeof(AppointmentCreatePage));
    }
}
