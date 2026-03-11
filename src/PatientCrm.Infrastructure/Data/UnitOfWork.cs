using PatientCrm.Core.Entities;
using PatientCrm.Core.Interfaces;
using PatientCrm.Infrastructure.Data;
using PatientCrm.Infrastructure.Repositories;

namespace PatientCrm.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private bool _disposed;

    public IPatientRepository Patients { get; }
    public IRepository<ClinicalNote> ClinicalNotes { get; }
    public IRepository<Appointment> Appointments { get; }
    public IRepository<Prescription> Prescriptions { get; }
    public IRepository<MedicalImage> MedicalImages { get; }
    public IRepository<PatientAlert> PatientAlerts { get; }
    public IRepository<DentalRecord> DentalRecords { get; }
    public IRepository<ToothRecord> ToothRecords { get; }
    public IRepository<GpRecord> GpRecords { get; }
    public IRepository<Department> Departments { get; }
    public IRepository<Ward> Wards { get; }
    public IRepository<PatientAdmission> PatientAdmissions { get; }
    public IRepository<Letter> Letters { get; }
    public IRepository<LetterTemplate> LetterTemplates { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Patients = new PatientRepository(context);
        ClinicalNotes = new Repository<ClinicalNote>(context);
        Appointments = new Repository<Appointment>(context);
        Prescriptions = new Repository<Prescription>(context);
        MedicalImages = new Repository<MedicalImage>(context);
        PatientAlerts = new Repository<PatientAlert>(context);
        DentalRecords = new Repository<DentalRecord>(context);
        ToothRecords = new Repository<ToothRecord>(context);
        GpRecords = new Repository<GpRecord>(context);
        Departments = new Repository<Department>(context);
        Wards = new Repository<Ward>(context);
        PatientAdmissions = new Repository<PatientAdmission>(context);
        Letters = new Repository<Letter>(context);
        LetterTemplates = new Repository<LetterTemplate>(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _context.DisposeAsync();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
