using PatientCrm.Core.Interfaces;

namespace PatientCrm.Api.Services;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; }
    public string TenantSlug { get; }

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var tenantIdClaim = user?.FindFirst("TenantId")?.Value;
        TenantId = Guid.TryParse(tenantIdClaim, out var id) ? id : Guid.Empty;
        TenantSlug = user?.FindFirst("TenantSlug")?.Value ?? string.Empty;
    }
}
