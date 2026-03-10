using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Web.Services;

namespace PatientCrm.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly PatientApiClient _api;

    public HomeController(PatientApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _api.GetDashboardStatsAsync();
        if (stats != null)
        {
            ViewBag.TotalPatients = stats.TotalPatients;
            ViewBag.TodayAppointments = stats.TodayAppointments;
            ViewBag.ActivePrescriptions = stats.ActivePrescriptions;
            ViewBag.ActiveAlerts = stats.ActiveAlerts;
            ViewBag.RecentPatients = stats.RecentPatients ?? [];
            ViewBag.UpcomingAppointments = stats.UpcomingAppointments ?? [];
        }
        else
        {
            ViewBag.TotalPatients = 0;
            ViewBag.TodayAppointments = 0;
            ViewBag.ActivePrescriptions = 0;
            ViewBag.ActiveAlerts = 0;
            ViewBag.RecentPatients = new List<PatientSummary>();
            ViewBag.UpcomingAppointments = new List<AppointmentSummary>();
        }

        return View();
    }
}
