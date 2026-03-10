using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Interfaces;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Infrastructure.Repositories;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Patient?> GetByNhsNumberAsync(string nhsNumber, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(p => p.NhsNumber == nhsNumber, cancellationToken);

    public async Task<Patient?> GetByHscniNumberAsync(string hscniNumber, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(p => p.HscniNumber == hscniNumber, cancellationToken);

    public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm, Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await _dbSet
            .Where(p => p.TenantId == tenantId &&
                (p.FirstName.ToLower().Contains(term) ||
                 p.LastName.ToLower().Contains(term) ||
                 (p.NhsNumber != null && p.NhsNumber.Contains(term)) ||
                 (p.HscniNumber != null && p.HscniNumber.Contains(term)) ||
                 (p.Email != null && p.Email.ToLower().Contains(term)) ||
                 (p.PhoneNumber != null && p.PhoneNumber.Contains(term))))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Patient?> GetWithFullRecordAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.ClinicalNotes)
                .ThenInclude(n => n.Images)
            .Include(p => p.Appointments)
            .Include(p => p.Prescriptions)
            .Include(p => p.MedicalImages)
            .Include(p => p.Alerts)
            .Include(p => p.Admissions)
                .ThenInclude(a => a.Department)
            .Include(p => p.Admissions)
                .ThenInclude(a => a.Ward)
            .Include(p => p.Admissions)
                .ThenInclude(a => a.Consultant)
            .Include(p => p.DentalRecord)
                .ThenInclude(d => d!.ToothRecords)
            .Include(p => p.GpRecord)
                .ThenInclude(g => g!.Referrals)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<int> GetTotalCountAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(p => p.TenantId == tenantId, cancellationToken);
}
