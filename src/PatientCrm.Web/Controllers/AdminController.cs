using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;
using PatientCrm.Web.Services;

namespace PatientCrm.Web.Controllers;

[Authorize(Roles = "SuperAdmin,TenantAdmin")]
public class AdminController : Controller
{
    private readonly PatientApiClient _api;

    public AdminController(PatientApiClient api)
    {
        _api = api;
    }

    private bool IsSuperAdmin() => User.IsInRole("SuperAdmin");

    // Dashboard
    public async Task<IActionResult> Index()
    {
        var stats = await _api.GetAdminStatsAsync();
        if (stats != null)
        {
            ViewBag.TenantCount = stats.TenantCount;
            ViewBag.UserCount = stats.UserCount;
            ViewBag.PatientCount = stats.PatientCount;
            ViewBag.GpCount = stats.GpCount;
            ViewBag.DentalCount = stats.DentalCount;
            ViewBag.HospitalCount = stats.HospitalCount;
            ViewBag.Tenants = stats.Tenants ?? [];
        }
        ViewBag.IsSuperAdmin = IsSuperAdmin();
        return View();
    }

    // Clients / Tenants
    public async Task<IActionResult> Clients(string? search, string? clientType)
    {
        var tenants = await _api.GetTenantsAsync(search, clientType) ?? [];
        ViewBag.Search = search;
        ViewBag.ClientTypeFilter = clientType;
        ViewBag.ClientTypes = Enum.GetValues<ClientType>();
        ViewBag.IsSuperAdmin = IsSuperAdmin();
        return View(tenants);
    }

    [Authorize(Roles = "SuperAdmin")]
    [Route("Admin/Clients/Create")]
    public IActionResult CreateClient()
    {
        ViewBag.TenantTypes = Enum.GetValues<TenantType>();
        ViewBag.ClientTypes = Enum.GetValues<ClientType>();
        ViewBag.DbProviders = Enum.GetValues<DatabaseProvider>();
        return View(new Tenant { Name = "", Slug = "" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    [Route("Admin/Clients/Create")]
    public async Task<IActionResult> CreateClient(Tenant tenant)
    {
        var created = await _api.CreateTenantAsync(tenant);
        if (created == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create client or slug already exists.");
            ViewBag.TenantTypes = Enum.GetValues<TenantType>();
            ViewBag.ClientTypes = Enum.GetValues<ClientType>();
            ViewBag.DbProviders = Enum.GetValues<DatabaseProvider>();
            return View(tenant);
        }

        TempData["SuccessMessage"] = $"Client '{created.Name}' created successfully.";
        return RedirectToAction(nameof(Clients));
    }

    [Route("Admin/Clients/Edit/{id:guid}")]
    public async Task<IActionResult> EditClient(Guid id)
    {
        var tenant = await _api.GetTenantAsync(id);
        if (tenant == null) return NotFound();

        ViewBag.TenantTypes = Enum.GetValues<TenantType>();
        ViewBag.ClientTypes = Enum.GetValues<ClientType>();
        ViewBag.DbProviders = Enum.GetValues<DatabaseProvider>();
        return View(tenant);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Clients/Edit/{id:guid}")]
    public async Task<IActionResult> EditClient(Guid id, Tenant tenant)
    {
        if (id != tenant.Id) return BadRequest();

        var success = await _api.UpdateTenantAsync(id, tenant);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update client.");
            ViewBag.TenantTypes = Enum.GetValues<TenantType>();
            ViewBag.ClientTypes = Enum.GetValues<ClientType>();
            ViewBag.DbProviders = Enum.GetValues<DatabaseProvider>();
            return View(tenant);
        }

        TempData["SuccessMessage"] = $"Client '{tenant.Name}' updated successfully.";
        return RedirectToAction(nameof(Clients));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    [Route("Admin/Clients/Toggle/{id:guid}")]
    public async Task<IActionResult> ToggleClientActive(Guid id)
    {
        await _api.ToggleTenantAsync(id);
        TempData["SuccessMessage"] = "Client status updated.";
        return RedirectToAction(nameof(Clients));
    }

    // Users
    public async Task<IActionResult> Users(Guid? tenantId)
    {
        var users = await _api.GetUsersAsync(tenantId) ?? [];
        ViewBag.SelectedTenantId = tenantId;
        ViewBag.Tenants = await _api.GetTenantsAsync(null, null) ?? [];
        ViewBag.IsSuperAdmin = IsSuperAdmin();
        return View(users);
    }

    // Legacy redirect
    public IActionResult Tenants() => RedirectToAction(nameof(Clients));
}
