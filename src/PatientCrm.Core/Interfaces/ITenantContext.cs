namespace PatientCrm.Core.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantSlug { get; }
}
