using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PatientCrm.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        Guid.TryParse(tenantIdClaim, out var tenantId);

        ViewBag.TotalPatients = await _context.Patients.CountAsync(p => p.TenantId == tenantId && !p.IsDeleted);
        ViewBag.TodayAppointments = await _context.Appointments
            .CountAsync(a => a.TenantId == tenantId && !a.IsDeleted && a.StartTime.Date == DateTime.Today);
        ViewBag.ActivePrescriptions = await _context.Prescriptions
            .CountAsync(p => p.TenantId == tenantId && !p.IsDeleted && p.Status == Core.Enums.PrescriptionStatus.Active);
        ViewBag.ActiveAlerts = await _context.PatientAlerts
            .CountAsync(a => a.TenantId == tenantId && !a.IsDeleted && a.IsActive);

        var recentPatients = await _context.Patients
            .Where(p => p.TenantId == tenantId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        var upcomingAppointments = await _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.TenantId == tenantId && !a.IsDeleted && a.StartTime >= DateTime.UtcNow)
            .OrderBy(a => a.StartTime)
            .Take(5)
            .ToListAsync();

        ViewBag.RecentPatients = recentPatients;
        ViewBag.UpcomingAppointments = upcomingAppointments;

        return View();
    }
}
