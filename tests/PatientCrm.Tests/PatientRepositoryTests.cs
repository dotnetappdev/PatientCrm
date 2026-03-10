using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;
using PatientCrm.Infrastructure.Data;
using PatientCrm.Infrastructure.Repositories;

namespace PatientCrm.Tests;

public class PatientRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PatientRepository _repository;
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public PatientRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new PatientRepository(_context);

        SeedData();
    }

    private void SeedData()
    {
        var tenant = new Tenant
        {
            Id = TenantId,
            Name = "Test Practice",
            Slug = "test-practice",
            TenantType = TenantType.NhsEngland
        };
        _context.Tenants.Add(tenant);

        var patients = new[]
        {
            new Patient
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1970, 1, 1),
                Gender = Gender.Male,
                NhsNumber = "943 476 0001",
                Status = PatientStatus.Active
            },
            new Patient
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = new DateTime(1985, 6, 15),
                Gender = Gender.Female,
                HscniNumber = "HC123456001",
                Status = PatientStatus.Active
            },
            new Patient
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                FirstName = "Bob",
                LastName = "Jones",
                DateOfBirth = new DateTime(1960, 3, 20),
                Gender = Gender.Male,
                NhsNumber = "943 476 0003",
                Status = PatientStatus.Active,
                IsDeleted = true // soft deleted
            }
        };

        _context.Patients.AddRange(patients);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedPatients()
    {
        var patients = await _repository.GetAllAsync();
        Assert.Equal(2, patients.Count());
    }

    [Fact]
    public async Task SearchAsync_ByFirstName_ShouldReturnMatchingPatients()
    {
        var results = await _repository.SearchAsync("John", TenantId, 1, 10);
        Assert.Single(results);
        Assert.Equal("John", results.First().FirstName);
    }

    [Fact]
    public async Task SearchAsync_ByNhsNumber_ShouldReturnMatchingPatient()
    {
        var results = await _repository.SearchAsync("943 476 0001", TenantId, 1, 10);
        Assert.Single(results);
        Assert.Equal("Smith", results.First().LastName);
    }

    [Fact]
    public async Task GetByNhsNumberAsync_ShouldReturnPatient()
    {
        var patient = await _repository.GetByNhsNumberAsync("943 476 0001");
        Assert.NotNull(patient);
        Assert.Equal("John", patient.FirstName);
    }

    [Fact]
    public async Task GetByHscniNumberAsync_ShouldReturnPatient()
    {
        var patient = await _repository.GetByHscniNumberAsync("HC123456001");
        Assert.NotNull(patient);
        Assert.Equal("Jane", patient.FirstName);
    }

    [Fact]
    public async Task AddAsync_ShouldAddNewPatient()
    {
        var newPatient = new Patient
        {
            TenantId = TenantId,
            FirstName = "Alice",
            LastName = "Brown",
            DateOfBirth = new DateTime(1995, 12, 1),
            Gender = Gender.Female,
            NhsNumber = "943 476 9999"
        };

        await _repository.AddAsync(newPatient);
        await _context.SaveChangesAsync();

        var count = await _repository.GetTotalCountAsync(TenantId);
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeletePatient()
    {
        var patients = await _repository.GetAllAsync();
        var patientId = patients.First().Id;

        await _repository.DeleteAsync(patientId);
        await _context.SaveChangesAsync();

        var patient = await _repository.GetByIdAsync(patientId);
        Assert.Null(patient); // Query filter should exclude deleted
    }

    [Fact]
    public async Task GetTotalCountAsync_ShouldExcludeDeletedPatients()
    {
        var count = await _repository.GetTotalCountAsync(TenantId);
        Assert.Equal(2, count);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
