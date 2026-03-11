using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;
using PatientCrm.Core.Interfaces;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LettersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ApplicationDbContext _context;

    public LettersController(IUnitOfWork unitOfWork, ITenantContext tenantContext, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _context = context;
    }

    // GET /api/letters?patientId=
    [HttpGet]
    public async Task<IActionResult> GetLetters([FromQuery] Guid? patientId, CancellationToken ct)
    {
        var isPatient = User.IsInRole("Patient");
        var letters = await _context.Letters
            .Where(l => !l.IsDeleted && l.TenantId == _tenantContext.TenantId)
            .ToListAsync(ct);

        if (patientId.HasValue)
            letters = letters.Where(l => l.PatientId == patientId.Value).ToList();

        if (isPatient)
        {
            // Patients only see letters marked as visible to them
            letters = letters.Where(l => l.IsPatientVisible).ToList();
        }

        return Ok(letters.OrderByDescending(l => l.LetterDate));
    }

    // GET /api/letters/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLetter(Guid id, CancellationToken ct)
    {
        var letter = await _unitOfWork.Letters.GetByIdAsync(id, ct);
        if (letter == null) return NotFound();

        if (User.IsInRole("Patient") && !letter.IsPatientVisible)
            return Forbid();

        return Ok(letter);
    }

    // POST /api/letters
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant")]
    public async Task<IActionResult> CreateLetter([FromBody] Letter letter, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        letter.TenantId = _tenantContext.TenantId;
        await _unitOfWork.Letters.AddAsync(letter, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetLetter), new { id = letter.Id }, letter);
    }

    // PUT /api/letters/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant")]
    public async Task<IActionResult> UpdateLetter(Guid id, [FromBody] Letter letter, CancellationToken ct)
    {
        if (id != letter.Id) return BadRequest();
        var existing = await _unitOfWork.Letters.GetByIdAsync(id, ct);
        if (existing == null) return NotFound();
        await _unitOfWork.Letters.UpdateAsync(letter, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE /api/letters/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> DeleteLetter(Guid id, CancellationToken ct)
    {
        await _unitOfWork.Letters.DeleteAsync(id, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // POST /api/letters/{id}/send
    [HttpPost("{id:guid}/send")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant")]
    public async Task<IActionResult> SendLetter(Guid id, CancellationToken ct)
    {
        var letter = await _unitOfWork.Letters.GetByIdAsync(id, ct);
        if (letter == null) return NotFound();
        letter.Status = LetterStatus.Sent;
        letter.SentAt = DateTime.UtcNow;
        await _unitOfWork.Letters.UpdateAsync(letter, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Ok(letter);
    }

    // GET /api/letters/templates
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates(CancellationToken ct)
    {
        var templates = await _context.LetterTemplates
            .Where(t => !t.IsDeleted && t.IsActive)
            .OrderBy(t => t.Category).ThenBy(t => t.Title)
            .ToListAsync(ct);
        return Ok(templates);
    }

    // POST /api/letters/templates
    [HttpPost("templates")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> CreateTemplate([FromBody] LetterTemplate template, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        template.TenantId = _tenantContext.TenantId;
        await _unitOfWork.LetterTemplates.AddAsync(template, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetTemplates), new { id = template.Id }, template);
    }

    // PUT /api/letters/templates/{id}
    [HttpPut("templates/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] LetterTemplate template, CancellationToken ct)
    {
        if (id != template.Id) return BadRequest();
        var existing = await _unitOfWork.LetterTemplates.GetByIdAsync(id, ct);
        if (existing == null) return NotFound();
        await _unitOfWork.LetterTemplates.UpdateAsync(template, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE /api/letters/templates/{id}
    [HttpDelete("templates/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken ct)
    {
        await _unitOfWork.LetterTemplates.DeleteAsync(id, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }
}
