using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Interfaces;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _env;

    public ImagesController(IUnitOfWork unitOfWork, ITenantContext tenantContext, IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _env = env;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken cancellationToken)
    {
        var image = await _unitOfWork.MedicalImages.GetByIdAsync(id, cancellationToken);
        if (image == null) return NotFound();
        return Ok(image);
    }

    [HttpPost("upload/{patientId:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse")]
    [RequestSizeLimit(104_857_600)] // 100 MB
    public async Task<IActionResult> UploadImage(
        Guid patientId,
        IFormFile file,
        [FromForm] string title,
        [FromForm] string imageType,
        [FromForm] string? description,
        [FromForm] Guid? clinicalNoteId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        if (!Enum.TryParse<Core.Enums.ImageType>(imageType, true, out var imgType))
            return BadRequest("Invalid image type");

        var patient = await _unitOfWork.Patients.GetByIdAsync(patientId, cancellationToken);
        if (patient == null) return NotFound("Patient not found");

        var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", patientId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var medicalImage = new MedicalImage
        {
            TenantId = _tenantContext.TenantId,
            PatientId = patientId,
            ClinicalNoteId = clinicalNoteId,
            Title = title,
            Description = description,
            ImageType = imgType,
            FileName = file.FileName,
            StoragePath = $"/uploads/{patientId}/{fileName}",
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            ImageDate = DateTime.UtcNow
        };

        await _unitOfWork.MedicalImages.AddAsync(medicalImage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetImage), new { id = medicalImage.Id }, medicalImage);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> DeleteImage(Guid id, CancellationToken cancellationToken)
    {
        var image = await _unitOfWork.MedicalImages.GetByIdAsync(id, cancellationToken);
        if (image == null) return NotFound();

        var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", image.StoragePath.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        await _unitOfWork.MedicalImages.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
