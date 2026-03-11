using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Api.Controllers;

/// <summary>
/// Patient-facing read-only API. All endpoints require the "Patient" role.
/// The caller's identity is resolved to their linked Patient record via PatientUserId.
/// Confidential clinical notes are never returned.
/// </summary>
[ApiController]
[Route("api/portal")]
[Authorize(Roles = "Patient")]
public class PatientPortalController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PatientPortalController(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid CallerUserId =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id : Guid.Empty;

    private async Task<PatientCrm.Core.Entities.Patient?> GetMyPatientAsync(CancellationToken ct)
        => await _context.Patients
            .Where(p => p.PatientUserId == CallerUserId && !p.IsDeleted)
            .Include(p => p.ClinicalNotes.Where(n => !n.IsConfidential && !n.IsDeleted))
            .Include(p => p.Appointments.Where(a => !a.IsDeleted))
            .Include(p => p.Prescriptions.Where(r => !r.IsDeleted))
            .Include(p => p.MedicalImages.Where(i => !i.IsDeleted))
            .Include(p => p.Alerts.Where(a => !a.IsDeleted))
            .Include(p => p.Admissions.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.Department)
            .Include(p => p.Admissions.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.Ward)
            .Include(p => p.GpRecord)
            .FirstOrDefaultAsync(ct);

    // GET /api/portal/me  — full patient record (no confidential notes)
    [HttpGet("me")]
    public async Task<IActionResult> GetMyRecord(CancellationToken ct)
    {
        var patient = await GetMyPatientAsync(ct);
        if (patient == null) return NotFound("No patient record is linked to your account. Please contact your practice.");
        return Ok(patient);
    }

    // GET /api/portal/me/appointments
    [HttpGet("me/appointments")]
    public async Task<IActionResult> GetMyAppointments(CancellationToken ct)
    {
        var patient = await GetMyPatientAsync(ct);
        if (patient == null) return NotFound();
        var appts = patient.Appointments.OrderByDescending(a => a.StartTime).ToList();
        return Ok(appts);
    }

    // GET /api/portal/me/prescriptions
    [HttpGet("me/prescriptions")]
    public async Task<IActionResult> GetMyPrescriptions(CancellationToken ct)
    {
        var patient = await GetMyPatientAsync(ct);
        if (patient == null) return NotFound();
        var prescriptions = patient.Prescriptions.OrderByDescending(p => p.PrescribedDate).ToList();
        return Ok(prescriptions);
    }

    // GET /api/portal/me/notes  — non-confidential notes only
    [HttpGet("me/notes")]
    public async Task<IActionResult> GetMyNotes(CancellationToken ct)
    {
        var patient = await GetMyPatientAsync(ct);
        if (patient == null) return NotFound();
        return Ok(patient.ClinicalNotes.OrderByDescending(n => n.NoteDate));
    }

    // GET /api/portal/me/admissions
    [HttpGet("me/admissions")]
    public async Task<IActionResult> GetMyAdmissions(CancellationToken ct)
    {
        var patient = await GetMyPatientAsync(ct);
        if (patient == null) return NotFound();
        return Ok(patient.Admissions.OrderByDescending(a => a.AdmissionDate));
    }

    // GET /api/portal/me/alerts
    [HttpGet("me/alerts")]
    public async Task<IActionResult> GetMyAlerts(CancellationToken ct)
    {
        var patient = await GetMyPatientAsync(ct);
        if (patient == null) return NotFound();
        return Ok(patient.Alerts.OrderByDescending(a => a.CreatedAt));
    }

    // GET /api/portal/me/letters  — patient-visible letters only
    [HttpGet("me/letters")]
    public async Task<IActionResult> GetMyLetters(CancellationToken ct)
    {
        var patient = await GetMyPatientAsync(ct);
        if (patient == null) return NotFound();
        var letters = await _context.Letters
            .Where(l => l.PatientId == patient.Id && l.IsPatientVisible && !l.IsDeleted)
            .OrderByDescending(l => l.LetterDate)
            .ToListAsync(ct);
        return Ok(letters);
    }
}
