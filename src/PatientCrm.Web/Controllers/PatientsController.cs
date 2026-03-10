using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;
using PatientCrm.Web.Services;

namespace PatientCrm.Web.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly PatientApiClient _api;

    public PatientsController(PatientApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
    {
        var result = await _api.GetPatientsAsync(search, page, pageSize) ?? new PagedPatientResult();

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = result.Total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)result.Total / pageSize);

        return View(result.Patients);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var patient = await _api.GetPatientAsync(id);
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

        var created = await _api.CreatePatientAsync(patient);
        if (created == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create patient. Please try again.");
            ViewBag.Genders = Enum.GetValues<Gender>();
            ViewBag.BloodGroups = Enum.GetValues<BloodGroup>();
            ViewBag.Regions = Enum.GetValues<NhsRegion>();
            return View(patient);
        }

        TempData["SuccessMessage"] = $"Patient {created.FullName} has been registered successfully.";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var patient = await _api.GetPatientAsync(id);
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

        var success = await _api.UpdatePatientAsync(id, patient);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update patient. Please try again.");
            ViewBag.Genders = Enum.GetValues<Gender>();
            ViewBag.BloodGroups = Enum.GetValues<BloodGroup>();
            ViewBag.Regions = Enum.GetValues<NhsRegion>();
            return View(patient);
        }

        TempData["SuccessMessage"] = "Patient record updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _api.DeletePatientAsync(id);
        TempData["SuccessMessage"] = "Patient record has been removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse")]
    public async Task<IActionResult> AddNote(Guid patientId, ClinicalNote note)
    {
        await _api.AddNoteAsync(patientId, note);
        TempData["SuccessMessage"] = "Clinical note added.";
        return RedirectToAction(nameof(Details), new { id = patientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,GP,Dentist,Consultant,Nurse")]
    [RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> UploadImage(Guid patientId, IFormFile file, string title, string imageType, string? description, Guid? clinicalNoteId)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "No file selected.";
            return RedirectToAction(nameof(Details), new { id = patientId });
        }

        var success = await _api.UploadImageAsync(patientId, file, title, imageType, description, clinicalNoteId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Image uploaded successfully." : "Failed to upload image.";
        return RedirectToAction(nameof(Details), new { id = patientId });
    }
}
