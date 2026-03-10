using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Enums;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirst("TenantId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var tenantId = GetTenantId();

        var totalPatients = await _context.Patients.CountAsync(p => p.TenantId == tenantId && !p.IsDeleted);
        var todayAppts = await _context.Appointments
            .CountAsync(a => a.TenantId == tenantId && !a.IsDeleted && a.StartTime.Date == DateTime.Today);
        var activePrescriptions = await _context.Prescriptions
            .CountAsync(p => p.TenantId == tenantId && !p.IsDeleted && p.Status == PrescriptionStatus.Active);
        var activeAlerts = await _context.PatientAlerts
            .CountAsync(a => a.TenantId == tenantId && !a.IsDeleted && a.IsActive);

        var recentPatients = await _context.Patients
            .Where(p => p.TenantId == tenantId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new
            {
                p.Id, p.FirstName, p.LastName, p.FullName, p.DateOfBirth,
                p.NhsNumber, p.HscniNumber, p.Gender, p.Status, p.CreatedAt
            })
            .ToListAsync();

        var upcomingAppointments = await _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.TenantId == tenantId && !a.IsDeleted && a.StartTime >= DateTime.UtcNow)
            .OrderBy(a => a.StartTime)
            .Take(5)
            .Select(a => new
            {
                a.Id,
                a.StartTime,
                a.EndTime,
                a.AppointmentType,
                a.Status,
                a.Title,
                PatientId = a.PatientId,
                PatientName = a.Patient != null ? a.Patient.FullName : null
            })
            .ToListAsync();

        return Ok(new
        {
            TotalPatients = totalPatients,
            TodayAppointments = todayAppts,
            ActivePrescriptions = activePrescriptions,
            ActiveAlerts = activeAlerts,
            RecentPatients = recentPatients,
            UpcomingAppointments = upcomingAppointments
        });
    }
}
