using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Interfaces;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ApplicationDbContext _context;

    public PatientsController(IUnitOfWork unitOfWork, ITenantContext tenantContext, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _context = context;
    }

    /// <summary>Returns true when the current caller is in the Patient role.</summary>
    private bool CallerIsPatient => User.IsInRole("Patient");

    /// <summary>Strips confidential clinical notes when the caller is a patient.</summary>
    private Patient StripConfidentialNotes(Patient patient)
    {
        if (CallerIsPatient)
        {
            // Return a shallow copy with confidential notes removed
            var visible = patient.ClinicalNotes.Where(n => !n.IsConfidential).ToList();
            patient.ClinicalNotes = visible;
        }
        return patient;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Receptionist,ReadOnly")]
    public async Task<IActionResult> GetPatients(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var results = await _unitOfWork.Patients.SearchAsync(search, _tenantContext.TenantId, page, pageSize, cancellationToken);
            return Ok(results);
        }

        var patients = await _unitOfWork.Patients.GetAllAsync(cancellationToken);
        return Ok(patients);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPatient(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetWithFullRecordAsync(id, cancellationToken);
        if (patient == null) return NotFound();

        // Patients can only access their own record
        if (CallerIsPatient)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (patient.PatientUserId?.ToString() != userId) return Forbid();
        }

        return Ok(StripConfidentialNotes(patient));
    }

    [HttpGet("nhs/{nhsNumber}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Receptionist,ReadOnly")]
    public async Task<IActionResult> GetByNhsNumber(string nhsNumber, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetByNhsNumberAsync(nhsNumber, cancellationToken);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    [HttpGet("hscni/{hscniNumber}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Receptionist,ReadOnly")]
    public async Task<IActionResult> GetByHscniNumber(string hscniNumber, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetByHscniNumberAsync(hscniNumber, cancellationToken);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Receptionist")]
    public async Task<IActionResult> CreatePatient([FromBody] Patient patient, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        patient.TenantId = _tenantContext.TenantId;
        await _unitOfWork.Patients.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Receptionist")]
    public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] Patient patient, CancellationToken cancellationToken)
    {
        if (id != patient.Id) return BadRequest();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _unitOfWork.Patients.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        await _unitOfWork.Patients.UpdateAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> DeletePatient(Guid id, CancellationToken cancellationToken)
    {
        await _unitOfWork.Patients.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/notes")]
    public async Task<IActionResult> GetPatientNotes(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetWithFullRecordAsync(id, cancellationToken);
        if (patient == null) return NotFound();

        if (CallerIsPatient)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (patient.PatientUserId?.ToString() != userId) return Forbid();
            return Ok(patient.ClinicalNotes.Where(n => !n.IsConfidential));
        }

        return Ok(patient.ClinicalNotes);
    }

    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetPatientImages(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetWithFullRecordAsync(id, cancellationToken);
        if (patient == null) return NotFound();
        return Ok(patient.MedicalImages);
    }

    [HttpGet("{id:guid}/appointments")]
    public async Task<IActionResult> GetPatientAppointments(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetWithFullRecordAsync(id, cancellationToken);
        if (patient == null) return NotFound();
        return Ok(patient.Appointments);
    }

    [HttpGet("{id:guid}/prescriptions")]
    public async Task<IActionResult> GetPatientPrescriptions(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetWithFullRecordAsync(id, cancellationToken);
        if (patient == null) return NotFound();
        return Ok(patient.Prescriptions);
    }

    /// <summary>
    /// POST /api/patients/{id}/portal-access
    /// Creates or returns a patient portal account for this patient.
    /// Only TenantAdmin / SuperAdmin / GP can invoke this.
    /// </summary>
    [HttpPost("{id:guid}/portal-access")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP")]
    public async Task<IActionResult> EnablePortalAccess(
        Guid id,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] RoleManager<ApplicationRole> roleManager,
        CancellationToken cancellationToken)
    {
        var patient = await _context.Patients.FindAsync(new object[] { id }, cancellationToken);
        if (patient == null) return NotFound("Patient not found.");

        if (patient.PatientUserId.HasValue)
        {
            var existing = await userManager.FindByIdAsync(patient.PatientUserId.Value.ToString());
            if (existing != null)
                return Ok(new { message = "Portal account already exists.", email = existing.Email });
        }

        if (string.IsNullOrWhiteSpace(patient.Email))
            return BadRequest("Patient must have an email address to enable portal access.");

        // Reuse or create the identity user
        var portalUser = await userManager.FindByEmailAsync(patient.Email);
        if (portalUser == null)
        {
            portalUser = new ApplicationUser
            {
                UserName = patient.Email,
                Email = patient.Email,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Title = "",
                TenantId = patient.TenantId,
                EmailConfirmed = true,
                IsActive = true
            };
            // Generate a cryptographically random temporary password and send a reset link
            // (In production, call a password-reset email flow instead of storing a temp password)
            var rng = System.Security.Cryptography.RandomNumberGenerator.GetBytes(24);
            var tempPassword = $"P@{Convert.ToBase64String(rng)[..12]}!1a";
            var result = await userManager.CreateAsync(portalUser, tempPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors.FirstOrDefault()?.Description ?? "Failed to create portal account.");
        }

        if (!await userManager.IsInRoleAsync(portalUser, "Patient"))
            await userManager.AddToRoleAsync(portalUser, "Patient");

        patient.PatientUserId = portalUser.Id;
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Portal access enabled.", email = portalUser.Email, userId = portalUser.Id });
    }
}
