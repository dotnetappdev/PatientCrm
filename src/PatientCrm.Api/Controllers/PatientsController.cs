using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Interfaces;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public PatientsController(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    [HttpGet]
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
        return Ok(patient);
    }

    [HttpGet("nhs/{nhsNumber}")]
    public async Task<IActionResult> GetByNhsNumber(string nhsNumber, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetByNhsNumberAsync(nhsNumber, cancellationToken);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    [HttpGet("hscni/{hscniNumber}")]
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
}
