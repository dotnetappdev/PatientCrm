using Microsoft.Extensions.Logging;
using PatientCrm.Maui.Converters;
using PatientCrm.Maui.Services;
using PatientCrm.Maui.ViewModels;
using PatientCrm.Maui.ViewModels.Portal;
using PatientCrm.Maui.Views;
using PatientCrm.Maui.Views.Appointments;
using PatientCrm.Maui.Views.Patients;
using PatientCrm.Maui.Views.Portal;

namespace PatientCrm.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // HTTP client
        builder.Services.AddHttpClient("PatientCrmApi", client =>
        {
            client.BaseAddress = new Uri(AppSettings.ApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Services
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ApiService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<PatientListViewModel>();
        builder.Services.AddTransient<PatientDetailViewModel>();
        builder.Services.AddTransient<PatientCreateViewModel>();
        builder.Services.AddTransient<AppointmentListViewModel>();
        builder.Services.AddTransient<AppointmentCreateViewModel>();
        builder.Services.AddTransient<AdminViewModel>();
        builder.Services.AddTransient<DepartmentListViewModel>();

        // Portal ViewModels
        builder.Services.AddTransient<PortalDashboardViewModel>();
        builder.Services.AddTransient<PortalAppointmentsViewModel>();
        builder.Services.AddTransient<PortalPrescriptionsViewModel>();
        builder.Services.AddTransient<PortalNotesViewModel>();
        builder.Services.AddTransient<PortalLettersViewModel>();
        builder.Services.AddTransient<PortalProfileViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<PatientListPage>();
        builder.Services.AddTransient<PatientDetailPage>();
        builder.Services.AddTransient<PatientCreatePage>();
        builder.Services.AddTransient<AppointmentListPage>();
        builder.Services.AddTransient<AppointmentCreatePage>();
        builder.Services.AddTransient<AdminPage>();
        builder.Services.AddTransient<DepartmentsPage>();

        // Portal Pages
        builder.Services.AddTransient<PortalDashboardPage>();
        builder.Services.AddTransient<PortalAppointmentsPage>();
        builder.Services.AddTransient<PortalPrescriptionsPage>();
        builder.Services.AddTransient<PortalNotesPage>();
        builder.Services.AddTransient<PortalLettersPage>();
        builder.Services.AddTransient<PortalProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
