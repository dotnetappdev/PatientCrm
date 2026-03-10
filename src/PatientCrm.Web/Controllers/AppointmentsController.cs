using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Web.Services;

namespace PatientCrm.Web.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly PatientApiClient _api;

    public AppointmentsController(PatientApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        var targetDate = date ?? DateTime.Today;
        var appointments = await _api.GetAppointmentsAsync(targetDate) ?? [];
        ViewBag.Date = targetDate;
        return View(appointments);
    }
}
