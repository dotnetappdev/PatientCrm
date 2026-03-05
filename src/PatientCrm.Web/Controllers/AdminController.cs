using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
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

    public async Task<IActionResult> Index()
    {
        ViewBag.TenantCount = await _context.Tenants.CountAsync(t => t.IsActive);
        ViewBag.UserCount = await _context.Users.CountAsync(u => u.IsActive);
        ViewBag.PatientCount = await _context.Patients.CountAsync(p => !p.IsDeleted);
        ViewBag.Tenants = await _context.Tenants.OrderBy(t => t.Name).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Users()
    {
        var users = await _context.Users
            .Include(u => u.Tenant)
            .OrderBy(u => u.LastName)
            .ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> Tenants()
    {
        var tenants = await _context.Tenants.OrderBy(t => t.Name).ToListAsync();
        return View(tenants);
    }
}
