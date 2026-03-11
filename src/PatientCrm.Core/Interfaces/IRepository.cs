using PatientCrm.Core.Entities;

namespace PatientCrm.Core.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByNhsNumberAsync(string nhsNumber, CancellationToken cancellationToken = default);
    Task<Patient?> GetByHscniNumberAsync(string hscniNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> SearchAsync(string searchTerm, Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Patient?> GetWithFullRecordAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetBySlugAsync(string slug);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<Tenant> AddAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
}

public interface IUnitOfWork : IAsyncDisposable
{
    IPatientRepository Patients { get; }
    IRepository<ClinicalNote> ClinicalNotes { get; }
    IRepository<Appointment> Appointments { get; }
    IRepository<Prescription> Prescriptions { get; }
    IRepository<MedicalImage> MedicalImages { get; }
    IRepository<PatientAlert> PatientAlerts { get; }
    IRepository<DentalRecord> DentalRecords { get; }
    IRepository<ToothRecord> ToothRecords { get; }
    IRepository<GpRecord> GpRecords { get; }
    IRepository<Department> Departments { get; }
    IRepository<Ward> Wards { get; }
    IRepository<PatientAdmission> PatientAdmissions { get; }
    IRepository<Letter> Letters { get; }
    IRepository<LetterTemplate> LetterTemplates { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
