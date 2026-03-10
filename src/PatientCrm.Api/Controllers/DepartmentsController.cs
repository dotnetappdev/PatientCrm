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
public class DepartmentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ApplicationDbContext _context;

    public DepartmentsController(IUnitOfWork unitOfWork, ITenantContext tenantContext, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _context = context;
    }

    // GET /api/departments
    [HttpGet]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .Where(d => d.TenantId == _tenantContext.TenantId && !d.IsDeleted)
            .Include(d => d.Wards.Where(w => !w.IsDeleted))
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
        return Ok(departments);
    }

    // GET /api/departments/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDepartment(Guid id, CancellationToken cancellationToken)
    {
        var dept = await _context.Departments
            .Where(d => d.Id == id && d.TenantId == _tenantContext.TenantId && !d.IsDeleted)
            .Include(d => d.Wards.Where(w => !w.IsDeleted))
            .Include(d => d.Admissions.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.Patient)
            .Include(d => d.Admissions.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.Consultant)
            .FirstOrDefaultAsync(cancellationToken);
        if (dept == null) return NotFound();
        return Ok(dept);
    }

    // POST /api/departments
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> CreateDepartment([FromBody] Department department, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        department.TenantId = _tenantContext.TenantId;
        await _unitOfWork.Departments.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
    }

    // PUT /api/departments/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] Department department, CancellationToken cancellationToken)
    {
        if (id != department.Id) return BadRequest();
        var existing = await _unitOfWork.Departments.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();
        await _unitOfWork.Departments.UpdateAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // DELETE /api/departments/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Departments.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();
        await _unitOfWork.Departments.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // GET /api/departments/{id}/wards
    [HttpGet("{id:guid}/wards")]
    public async Task<IActionResult> GetWards(Guid id, CancellationToken cancellationToken)
    {
        var wards = await _context.Wards
            .Where(w => w.DepartmentId == id && !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
        return Ok(wards);
    }

    // POST /api/departments/{id}/wards
    [HttpPost("{id:guid}/wards")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> CreateWard(Guid id, [FromBody] Ward ward, CancellationToken cancellationToken)
    {
        var dept = await _unitOfWork.Departments.GetByIdAsync(id, cancellationToken);
        if (dept == null) return NotFound("Department not found.");
        ward.DepartmentId = id;
        ward.TenantId = _tenantContext.TenantId;
        await _unitOfWork.Wards.AddAsync(ward, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetWards), new { id }, ward);
    }

    // PUT /api/departments/wards/{wardId}
    [HttpPut("wards/{wardId:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> UpdateWard(Guid wardId, [FromBody] Ward ward, CancellationToken cancellationToken)
    {
        if (wardId != ward.Id) return BadRequest();
        var existing = await _unitOfWork.Wards.GetByIdAsync(wardId, cancellationToken);
        if (existing == null) return NotFound();
        await _unitOfWork.Wards.UpdateAsync(ward, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // DELETE /api/departments/wards/{wardId}
    [HttpDelete("wards/{wardId:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> DeleteWard(Guid wardId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Wards.GetByIdAsync(wardId, cancellationToken);
        if (existing == null) return NotFound();
        await _unitOfWork.Wards.DeleteAsync(wardId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // GET /api/departments/admissions?patientId=...
    [HttpGet("admissions")]
    public async Task<IActionResult> GetAdmissions([FromQuery] Guid? patientId, [FromQuery] Guid? departmentId, CancellationToken cancellationToken)
    {
        var query = _context.PatientAdmissions
            .Where(a => a.TenantId == _tenantContext.TenantId && !a.IsDeleted)
            .Include(a => a.Patient)
            .Include(a => a.Department)
            .Include(a => a.Ward)
            .Include(a => a.Consultant)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(a => a.PatientId == patientId.Value);
        if (departmentId.HasValue) query = query.Where(a => a.DepartmentId == departmentId.Value);

        var admissions = await query.OrderByDescending(a => a.AdmissionDate).ToListAsync(cancellationToken);
        return Ok(admissions);
    }

    // POST /api/departments/admissions
    [HttpPost("admissions")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Nurse,Consultant")]
    public async Task<IActionResult> CreateAdmission([FromBody] PatientAdmission admission, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        admission.TenantId = _tenantContext.TenantId;
        await _unitOfWork.PatientAdmissions.AddAsync(admission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Created($"/api/departments/admissions/{admission.Id}", admission);
    }

    // PUT /api/departments/admissions/{admissionId}
    [HttpPut("admissions/{admissionId:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Nurse,Consultant")]
    public async Task<IActionResult> UpdateAdmission(Guid admissionId, [FromBody] PatientAdmission admission, CancellationToken cancellationToken)
    {
        if (admissionId != admission.Id) return BadRequest();
        var existing = await _unitOfWork.PatientAdmissions.GetByIdAsync(admissionId, cancellationToken);
        if (existing == null) return NotFound();
        await _unitOfWork.PatientAdmissions.UpdateAsync(admission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // DELETE /api/departments/admissions/{admissionId}
    [HttpDelete("admissions/{admissionId:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> DeleteAdmission(Guid admissionId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.PatientAdmissions.GetByIdAsync(admissionId, cancellationToken);
        if (existing == null) return NotFound();
        await _unitOfWork.PatientAdmissions.DeleteAsync(admissionId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
