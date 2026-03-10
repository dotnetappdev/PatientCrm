using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Interfaces;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public NotesController(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNote(Guid id, CancellationToken cancellationToken)
    {
        var note = await _unitOfWork.ClinicalNotes.GetByIdAsync(id, cancellationToken);
        if (note == null) return NotFound();
        return Ok(note);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse")]
    public async Task<IActionResult> CreateNote([FromBody] ClinicalNote note, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        note.TenantId = _tenantContext.TenantId;
        await _unitOfWork.ClinicalNotes.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse")]
    public async Task<IActionResult> UpdateNote(Guid id, [FromBody] ClinicalNote note, CancellationToken cancellationToken)
    {
        if (id != note.Id) return BadRequest();
        var existing = await _unitOfWork.ClinicalNotes.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();
        if (existing.IsLocked) return Forbid();
        await _unitOfWork.ClinicalNotes.UpdateAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> DeleteNote(Guid id, CancellationToken cancellationToken)
    {
        await _unitOfWork.ClinicalNotes.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
