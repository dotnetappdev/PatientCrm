using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Web.Controllers;

[Authorize(Roles = "SuperAdmin,TenantAdmin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private bool IsSuperAdmin() => User.IsInRole("SuperAdmin");

    private Guid GetTenantId()
    {
        var claim = User.FindFirst("TenantId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // ── Dashboard ──────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        IQueryable<Tenant> tenantsQ = _context.Tenants;
        if (!IsSuperAdmin())
        {
            var tid = GetTenantId();
            tenantsQ = tenantsQ.Where(t => t.Id == tid);
        }

        ViewBag.TenantCount = await tenantsQ.CountAsync(t => t.IsActive);
        ViewBag.UserCount = IsSuperAdmin()
            ? await _context.Users.CountAsync(u => u.IsActive)
            : await _context.Users.CountAsync(u => u.IsActive && u.TenantId == GetTenantId());
        ViewBag.PatientCount = IsSuperAdmin()
            ? await _context.Patients.CountAsync(p => !p.IsDeleted)
            : await _context.Patients.CountAsync(p => !p.IsDeleted && p.TenantId == GetTenantId());

        ViewBag.GpCount = await tenantsQ.CountAsync(t => t.ClientType == ClientType.GpPractice && t.IsActive);
        ViewBag.DentalCount = await tenantsQ.CountAsync(t => t.ClientType == ClientType.DentalPractice && t.IsActive);
        ViewBag.HospitalCount = await tenantsQ.CountAsync(t => t.ClientType == ClientType.HospitalConsulting && t.IsActive);

        ViewBag.Tenants = await tenantsQ.OrderBy(t => t.Name).ToListAsync();
        ViewBag.IsSuperAdmin = IsSuperAdmin();
        return View();
    }

    // ── Clients / Tenants ──────────────────────────────────────────────────────
    public async Task<IActionResult> Clients(string? search, string? clientType)
    {
        IQueryable<Tenant> query = _context.Tenants;
        if (!IsSuperAdmin())
            query = query.Where(t => t.Id == GetTenantId());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                (t.OdsCode != null && t.OdsCode.ToLower().Contains(term)) ||
                (t.City != null && t.City.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(clientType) && Enum.TryParse<ClientType>(clientType, out var ct))
            query = query.Where(t => t.ClientType == ct);

        var tenants = await query.OrderBy(t => t.Name).ToListAsync();
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
        // Auto-generate slug if empty
        if (string.IsNullOrWhiteSpace(tenant.Slug))
            tenant.Slug = tenant.Name.ToLower().Replace(" ", "-").Replace("'", "");

        // Ensure slug uniqueness
        if (await _context.Tenants.AnyAsync(t => t.Slug == tenant.Slug))
        {
            ModelState.AddModelError("Slug", "A client with this slug already exists.");
            ViewBag.TenantTypes = Enum.GetValues<TenantType>();
            ViewBag.ClientTypes = Enum.GetValues<ClientType>();
            ViewBag.DbProviders = Enum.GetValues<DatabaseProvider>();
            return View(tenant);
        }

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Client '{tenant.Name}' created successfully.";
        return RedirectToAction(nameof(Clients));
    }

    [Route("Admin/Clients/Edit/{id:guid}")]
    public async Task<IActionResult> EditClient(Guid id)
    {
        var tenant = await GetTenantForEdit(id);
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

        var existing = await GetTenantForEdit(id);
        if (existing == null) return NotFound();

        existing.Name = tenant.Name;
        existing.TenantType = tenant.TenantType;
        existing.ClientType = tenant.ClientType;
        existing.OdsCode = tenant.OdsCode;
        existing.Address = tenant.Address;
        existing.City = tenant.City;
        existing.Postcode = tenant.Postcode;
        existing.PhoneNumber = tenant.PhoneNumber;
        existing.Email = tenant.Email;
        existing.Website = tenant.Website;
        existing.ContactName = tenant.ContactName;
        existing.Notes = tenant.Notes;
        existing.IsActive = tenant.IsActive;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Client '{existing.Name}' updated successfully.";
        return RedirectToAction(nameof(Clients));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    [Route("Admin/Clients/Toggle/{id:guid}")]
    public async Task<IActionResult> ToggleClientActive(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();
        tenant.IsActive = !tenant.IsActive;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Client '{tenant.Name}' {(tenant.IsActive ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Clients));
    }

    // ── Users ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Users(Guid? tenantId)
    {
        IQueryable<ApplicationUser> query = _context.Users.Include(u => u.Tenant);

        if (!IsSuperAdmin())
            query = query.Where(u => u.TenantId == GetTenantId());
        else if (tenantId.HasValue)
            query = query.Where(u => u.TenantId == tenantId.Value);

        var users = await query.OrderBy(u => u.LastName).ToListAsync();
        ViewBag.SelectedTenantId = tenantId;
        ViewBag.Tenants = await _context.Tenants.OrderBy(t => t.Name).ToListAsync();
        ViewBag.IsSuperAdmin = IsSuperAdmin();
        return View(users);
    }

    // ── Tenants (legacy redirect) ──────────────────────────────────────────────
    public IActionResult Tenants() => RedirectToAction(nameof(Clients));

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task<Tenant?> GetTenantForEdit(Guid id)
    {
        if (IsSuperAdmin())
            return await _context.Tenants.FindAsync(id);

        var myId = GetTenantId();
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id && t.Id == myId);
    }
}
