using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;

namespace PatientCrm.Tests;

public class PatientEntityTests
{
    [Fact]
    public void Patient_FullName_ShouldCombineFirstAndLastName()
    {
        var patient = new Patient
        {
            FirstName = "John",
            LastName = "Smith",
            DateOfBirth = new DateTime(1970, 1, 1),
            Gender = Gender.Male
        };

        Assert.Equal("John Smith", patient.FullName);
    }

    [Fact]
    public void Patient_FullName_WithMiddleName_ShouldIncludeMiddleName()
    {
        var patient = new Patient
        {
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Smith",
            DateOfBirth = new DateTime(1970, 1, 1),
            Gender = Gender.Male
        };

        Assert.Equal("John Michael Smith", patient.FullName);
    }

    [Fact]
    public void ApplicationUser_FullName_ShouldCombineNames()
    {
        var user = new ApplicationUser
        {
            FirstName = "Dr. Sarah",
            LastName = "Jones"
        };

        Assert.Equal("Dr. Sarah Jones", user.FullName);
    }

    [Fact]
    public void BaseEntity_ShouldHaveDefaultValues()
    {
        var patient = new Patient
        {
            FirstName = "Test",
            LastName = "Patient",
            DateOfBirth = DateTime.Today,
            Gender = Gender.Female
        };

        Assert.NotEqual(Guid.Empty, patient.Id);
        Assert.False(patient.IsDeleted);
        Assert.True(patient.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Tenant_ShouldHaveDefaultValues()
    {
        var tenant = new Tenant
        {
            Name = "Test Practice",
            Slug = "test-practice",
            TenantType = TenantType.NhsEngland
        };

        Assert.True(tenant.IsActive);
        Assert.NotEqual(Guid.Empty, tenant.Id);
    }

    [Fact]
    public void ClinicalNote_ShouldHaveDefaultNoteDate()
    {
        var note = new ClinicalNote
        {
            Title = "Test Note",
            Content = "Test content",
            AuthorId = Guid.NewGuid(),
            AuthorName = "Dr. Test"
        };

        Assert.True(note.NoteDate <= DateTime.UtcNow);
        Assert.False(note.IsConfidential);
        Assert.False(note.IsLocked);
    }

    [Fact]
    public void Appointment_DurationMinutes_ShouldBeCorrect()
    {
        var start = new DateTime(2024, 1, 15, 9, 0, 0);
        var end = new DateTime(2024, 1, 15, 9, 30, 0);

        var appt = new Appointment
        {
            Title = "Test Appointment",
            StartTime = start,
            EndTime = end,
            ProviderId = Guid.NewGuid(),
            PatientId = Guid.NewGuid()
        };

        Assert.Equal(30, appt.DurationMinutes);
    }
}
