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
}
