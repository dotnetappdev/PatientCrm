using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Web.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirst("TenantId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        var tenantId = GetTenantId();
        var targetDate = date ?? DateTime.Today;

        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Where(a => a.TenantId == tenantId && !a.IsDeleted && a.StartTime.Date == targetDate)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        ViewBag.Date = targetDate;
        return View(appointments);
    }
}
