using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Interfaces;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DentalController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ApplicationDbContext _context;

    public DentalController(IUnitOfWork unitOfWork, ITenantContext tenantContext, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _context = context;
    }

    // GET /api/dental/{patientId} — get or create dental record for patient
    [HttpGet("{patientId:guid}")]
    public async Task<IActionResult> GetDentalRecord(Guid patientId, CancellationToken ct)
    {
        var record = await _context.DentalRecords
            .Include(d => d.ToothRecords.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(d => d.PatientId == patientId && !d.IsDeleted, ct);

        if (record == null)
        {
            // Auto-create a blank dental record
            record = new DentalRecord
            {
                PatientId = patientId,
                TenantId = _tenantContext.TenantId
            };
            await _unitOfWork.DentalRecords.AddAsync(record, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return Ok(record);
    }

    // PUT /api/dental/{id} — update dental record
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist")]
    public async Task<IActionResult> UpdateDentalRecord(Guid id, [FromBody] DentalRecord record, CancellationToken ct)
    {
        if (id != record.Id) return BadRequest();
        var existing = await _unitOfWork.DentalRecords.GetByIdAsync(id, ct);
        if (existing == null) return NotFound();
        await _unitOfWork.DentalRecords.UpdateAsync(record, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // GET /api/dental/{patientId}/teeth — list tooth records
    [HttpGet("{patientId:guid}/teeth")]
    public async Task<IActionResult> GetToothRecords(Guid patientId, CancellationToken ct)
    {
        var record = await _context.DentalRecords
            .Include(d => d.ToothRecords.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(d => d.PatientId == patientId && !d.IsDeleted, ct);

        if (record == null) return Ok(new List<ToothRecord>());
        return Ok(record.ToothRecords.OrderBy(t => t.ToothNumber));
    }

    // POST /api/dental/{patientId}/teeth — create or upsert tooth record
    [HttpPost("{patientId:guid}/teeth")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,Dentist")]
    public async Task<IActionResult> SaveToothRecord(Guid patientId, [FromBody] ToothRecord tooth, CancellationToken ct)
    {
        var record = await _context.DentalRecords
            .FirstOrDefaultAsync(d => d.PatientId == patientId && !d.IsDeleted, ct);

        if (record == null)
        {
            record = new DentalRecord { PatientId = patientId, TenantId = _tenantContext.TenantId };
            await _unitOfWork.DentalRecords.AddAsync(record, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // Check if a tooth record for this tooth number already exists
        var existing = await _context.ToothRecords
            .FirstOrDefaultAsync(t => t.DentalRecordId == record.Id && t.ToothNumber == tooth.ToothNumber && !t.IsDeleted, ct);

        if (existing != null)
        {
            // Update existing
            tooth.Id = existing.Id;
            tooth.DentalRecordId = existing.DentalRecordId;
            tooth.TenantId = existing.TenantId;
            await _unitOfWork.ToothRecords.UpdateAsync(tooth, ct);
        }
        else
        {
            tooth.DentalRecordId = record.Id;
            tooth.TenantId = _tenantContext.TenantId;
            await _unitOfWork.ToothRecords.AddAsync(tooth, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Ok(tooth);
    }

    // PUT /api/dental/tooth/{id} — update individual tooth record
    [HttpPut("tooth/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,Dentist")]
    public async Task<IActionResult> UpdateToothRecord(Guid id, [FromBody] ToothRecord tooth, CancellationToken ct)
    {
        if (id != tooth.Id) return BadRequest();
        var existing = await _unitOfWork.ToothRecords.GetByIdAsync(id, ct);
        if (existing == null) return NotFound();
        await _unitOfWork.ToothRecords.UpdateAsync(tooth, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }
}
