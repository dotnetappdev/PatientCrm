using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,TenantAdmin")]
public class AdminController : ControllerBase
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

    // Dashboard stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        IQueryable<Tenant> tenantsQ = _context.Tenants;
        if (!IsSuperAdmin())
            tenantsQ = tenantsQ.Where(t => t.Id == GetTenantId());

        var tenantId = GetTenantId();

        return Ok(new
        {
            TenantCount = await tenantsQ.CountAsync(t => t.IsActive),
            UserCount = IsSuperAdmin()
                ? await _context.Users.CountAsync(u => u.IsActive)
                : await _context.Users.CountAsync(u => u.IsActive && u.TenantId == tenantId),
            PatientCount = IsSuperAdmin()
                ? await _context.Patients.CountAsync(p => !p.IsDeleted)
                : await _context.Patients.CountAsync(p => !p.IsDeleted && p.TenantId == tenantId),
            GpCount = await tenantsQ.CountAsync(t => t.ClientType == ClientType.GpPractice && t.IsActive),
            DentalCount = await tenantsQ.CountAsync(t => t.ClientType == ClientType.DentalPractice && t.IsActive),
            HospitalCount = await tenantsQ.CountAsync(t => t.ClientType == ClientType.HospitalConsulting && t.IsActive),
            Tenants = await tenantsQ.OrderBy(t => t.Name).Select(t => new
            {
                t.Id, t.Name, t.Slug, t.ClientType, t.TenantType, t.OdsCode, t.City, t.IsActive, t.CreatedAt
            }).ToListAsync()
        });
    }

    // Tenants

    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants([FromQuery] string? search, [FromQuery] string? clientType)
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
        return Ok(tenants);
    }

    [HttpGet("tenants/{id:guid}")]
    public async Task<IActionResult> GetTenant(Guid id)
    {
        var tenant = await GetTenantForEdit(id);
        if (tenant == null) return NotFound();
        return Ok(tenant);
    }

    [HttpPost("tenants")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateTenant([FromBody] Tenant tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant.Slug))
            tenant.Slug = tenant.Name.ToLower().Replace(" ", "-").Replace("'", "");

        if (await _context.Tenants.AnyAsync(t => t.Slug == tenant.Slug))
            return Conflict(new { message = "A client with this slug already exists." });

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
    }

    [HttpPut("tenants/{id:guid}")]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] Tenant tenant)
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
        return Ok(existing);
    }

    [HttpPost("tenants/{id:guid}/toggle")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ToggleTenant(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();
        tenant.IsActive = !tenant.IsActive;
        await _context.SaveChangesAsync();
        return Ok(new { tenant.Id, tenant.Name, tenant.IsActive });
    }

    // Users

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] Guid? tenantId)
    {
        IQueryable<ApplicationUser> query = _context.Users.Include(u => u.Tenant);

        if (!IsSuperAdmin())
            query = query.Where(u => u.TenantId == GetTenantId());
        else if (tenantId.HasValue)
            query = query.Where(u => u.TenantId == tenantId.Value);

        var users = await query.OrderBy(u => u.LastName).Select(u => new
        {
            u.Id, u.Email, u.FirstName, u.LastName, u.FullName,
            u.TenantId, u.IsActive, u.TwoFactorEnabled, u.LastLoginAt, u.CreatedAt,
            u.GmcNumber, u.GdcNumber, u.Title,
            TenantName = u.Tenant != null ? u.Tenant.Name : null
        }).ToListAsync();

        return Ok(users);
    }

    [HttpPost("users/{userId:guid}/toggle")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> ToggleUser(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!IsSuperAdmin() && user.TenantId != GetTenantId())
            return Forbid();

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        return Ok(new { user.Id, user.FullName, user.IsActive });
    }

    [HttpGet("users/{userId:guid}/roles")]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        if (!IsSuperAdmin() && user.TenantId != GetTenantId())
            return Forbid();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(roles);
    }

    [HttpPost("users/{userId:guid}/roles")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> SetUserRole(Guid userId, [FromBody] SetRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        if (!IsSuperAdmin() && user.TenantId != GetTenantId())
            return Forbid();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var result = await _userManager.AddToRoleAsync(user, request.Role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = $"Role set to {request.Role}" });
    }

    private async Task<Tenant?> GetTenantForEdit(Guid id)
    {
        if (IsSuperAdmin())
            return await _context.Tenants.FindAsync(id);
        var myId = GetTenantId();
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id && t.Id == myId);
    }
}

public record SetRoleRequest(string Role);
