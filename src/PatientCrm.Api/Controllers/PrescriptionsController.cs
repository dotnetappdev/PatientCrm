using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Interfaces;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public PrescriptionsController(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPrescription(Guid id, CancellationToken cancellationToken)
    {
        var prescription = await _unitOfWork.Prescriptions.GetByIdAsync(id, cancellationToken);
        if (prescription == null) return NotFound();
        return Ok(prescription);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant")]
    public async Task<IActionResult> CreatePrescription([FromBody] Prescription prescription, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        prescription.TenantId = _tenantContext.TenantId;
        await _unitOfWork.Prescriptions.AddAsync(prescription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetPrescription), new { id = prescription.Id }, prescription);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant")]
    public async Task<IActionResult> UpdatePrescription(Guid id, [FromBody] Prescription prescription, CancellationToken cancellationToken)
    {
        if (id != prescription.Id) return BadRequest();
        var existing = await _unitOfWork.Prescriptions.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();
        await _unitOfWork.Prescriptions.UpdateAsync(prescription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP")]
    public async Task<IActionResult> DeletePrescription(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Prescriptions.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();
        await _unitOfWork.Prescriptions.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reorder")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Patient")]
    public async Task<IActionResult> ReorderPrescription(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Prescriptions.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();
        if (existing.Status != PatientCrm.Core.Enums.PrescriptionStatus.Active)
            return BadRequest("Only active prescriptions can be reordered.");
        if (existing.RepeatsRemaining.HasValue && existing.RepeatsRemaining <= 0)
            return BadRequest("No repeats remaining on this prescription. Please contact your prescriber.");

        // Create a new prescription as a reorder copy
        var reorder = new Prescription
        {
            PatientId = existing.PatientId,
            PrescriberId = existing.PrescriberId,
            MedicationName = existing.MedicationName,
            GenericName = existing.GenericName,
            Dosage = existing.Dosage,
            Frequency = existing.Frequency,
            Route = existing.Route,
            Instructions = existing.Instructions,
            QuantityIssued = existing.QuantityIssued,
            Unit = existing.Unit,
            Repeats = existing.Repeats,
            RepeatsRemaining = existing.RepeatsRemaining.HasValue ? existing.RepeatsRemaining - 1 : null,
            SnomedCode = existing.SnomedCode,
            DmdCode = existing.DmdCode,
            IsControlledDrug = existing.IsControlledDrug,
            Indication = existing.Indication,
            PrescribedDate = DateTime.UtcNow,
            StartDate = DateTime.UtcNow,
            Status = PatientCrm.Core.Enums.PrescriptionStatus.Active,
            TenantId = existing.TenantId
        };

        // Decrement repeats on the original
        if (existing.RepeatsRemaining.HasValue)
        {
            existing.RepeatsRemaining -= 1;
            await _unitOfWork.Prescriptions.UpdateAsync(existing, cancellationToken);
        }

        await _unitOfWork.Prescriptions.AddAsync(reorder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetPrescription), new { id = reorder.Id }, reorder);
    }
}
