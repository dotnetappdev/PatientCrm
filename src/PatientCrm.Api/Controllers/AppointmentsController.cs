using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Interfaces;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AppointmentsController(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments(CancellationToken cancellationToken)
    {
        var appointments = await _unitOfWork.Appointments.GetAllAsync(cancellationToken);
        return Ok(appointments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAppointment(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Receptionist")]
    public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        appointment.TenantId = _tenantContext.TenantId;
        await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Receptionist")]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] Appointment appointment, CancellationToken cancellationToken)
    {
        if (id != appointment.Id) return BadRequest();
        var existing = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();
        await _unitOfWork.Appointments.UpdateAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,Receptionist")]
    public async Task<IActionResult> CancelAppointment(Guid id, CancellationToken cancellationToken)
    {
        await _unitOfWork.Appointments.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
