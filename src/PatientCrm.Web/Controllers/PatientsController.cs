using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Web.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PatientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirst("TenantId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
    {
        var tenantId = GetTenantId();
        var query = _context.Patients
            .Where(p => p.TenantId == tenantId && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                (p.NhsNumber != null && p.NhsNumber.Contains(term)) ||
                (p.HscniNumber != null && p.HscniNumber.Contains(term)));
        }

        var total = await query.CountAsync();
        var patients = await query
            .OrderBy(p => p.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

        return View(patients);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var tenantId = GetTenantId();
        var patient = await _context.Patients
            .Include(p => p.ClinicalNotes.Where(n => !n.IsDeleted))
                .ThenInclude(n => n.Images.Where(i => !i.IsDeleted))
            .Include(p => p.Appointments.Where(a => !a.IsDeleted))
            .Include(p => p.Prescriptions.Where(pr => !pr.IsDeleted))
            .Include(p => p.MedicalImages.Where(i => !i.IsDeleted))
            .Include(p => p.Alerts.Where(a => !a.IsDeleted))
            .Include(p => p.DentalRecord)
                .ThenInclude(d => d!.ToothRecords.Where(t => !t.IsDeleted))
            .Include(p => p.GpRecord)
                .ThenInclude(g => g!.Referrals.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && !p.IsDeleted);

        if (patient == null) return NotFound();
        return View(patient);
    }

    public IActionResult Create()
    {
        ViewBag.Genders = Enum.GetValues<Gender>();
        ViewBag.BloodGroups = Enum.GetValues<BloodGroup>();
        ViewBag.Regions = Enum.GetValues<NhsRegion>();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Receptionist")]
    public async Task<IActionResult> Create(Patient patient)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Genders = Enum.GetValues<Gender>();
            ViewBag.BloodGroups = Enum.GetValues<BloodGroup>();
            ViewBag.Regions = Enum.GetValues<NhsRegion>();
            return View(patient);
        }

        patient.TenantId = GetTenantId();
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Patient {patient.FullName} has been registered successfully.";
        return RedirectToAction(nameof(Details), new { id = patient.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var tenantId = GetTenantId();
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (patient == null) return NotFound();

        ViewBag.Genders = Enum.GetValues<Gender>();
        ViewBag.BloodGroups = Enum.GetValues<BloodGroup>();
        ViewBag.Regions = Enum.GetValues<NhsRegion>();
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse,Receptionist")]
    public async Task<IActionResult> Edit(Guid id, Patient patient)
    {
        if (id != patient.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Genders = Enum.GetValues<Gender>();
            ViewBag.BloodGroups = Enum.GetValues<BloodGroup>();
            ViewBag.Regions = Enum.GetValues<NhsRegion>();
            return View(patient);
        }

        var tenantId = GetTenantId();
        patient.TenantId = tenantId;
        patient.UpdatedAt = DateTime.UtcNow;
        _context.Update(patient);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Patient record updated successfully.";
        return RedirectToAction(nameof(Details), new { id = patient.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (patient != null)
        {
            patient.IsDeleted = true;
            patient.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Patient {patient.FullName} has been removed.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Add Clinical Note
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse")]
    public async Task<IActionResult> AddNote(Guid patientId, ClinicalNote note)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.Users.FindAsync(Guid.Parse(userId!));

        note.PatientId = patientId;
        note.TenantId = GetTenantId();
        note.AuthorId = Guid.Parse(userId!);
        note.AuthorName = user?.FullName ?? User.Identity?.Name ?? "Unknown";
        note.NoteDate = DateTime.UtcNow;

        _context.ClinicalNotes.Add(note);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Clinical note added.";
        return RedirectToAction(nameof(Details), new { id = patientId });
    }

    // Upload image
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse")]
    [RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> UploadImage(Guid patientId, IFormFile file, string title, string imageType, string? description, Guid? clinicalNoteId, IWebHostEnvironment env)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "No file selected.";
            return RedirectToAction(nameof(Details), new { id = patientId });
        }

        if (!Enum.TryParse<ImageType>(imageType, true, out var imgType))
            imgType = ImageType.Photograph;

        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", patientId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var image = new MedicalImage
        {
            TenantId = GetTenantId(),
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

        _context.MedicalImages.Add(image);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Image uploaded successfully.";
        return RedirectToAction(nameof(Details), new { id = patientId });
    }
}
