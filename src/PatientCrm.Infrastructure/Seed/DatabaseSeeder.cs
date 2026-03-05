using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;
using PatientCrm.Infrastructure.Data;

namespace PatientCrm.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed tenants
        var tenants = await SeedTenantsAsync(context);

        // Seed users
        await SeedUsersAsync(userManager, tenants);

        // Seed patients
        await SeedPatientsAsync(context, tenants);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roles = ["SuperAdmin", "TenantAdmin", "GP", "Dentist", "Consultant", "Nurse", "Receptionist", "ReadOnly"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    Description = $"{role} role",
                    NormalizedName = role.ToUpper()
                });
            }
        }
    }

    private static async Task<List<Tenant>> SeedTenantsAsync(ApplicationDbContext context)
    {
        if (await context.Tenants.AnyAsync())
            return await context.Tenants.ToListAsync();

        var tenants = new List<Tenant>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Greenfield Medical Practice",
                Slug = "greenfield-medical",
                TenantType = TenantType.NhsEngland,
                OdsCode = "A81001",
                Address = "1 High Street",
                City = "Manchester",
                Postcode = "M1 1AA",
                PhoneNumber = "0161 000 0001",
                Email = "admin@greenfield.nhs.uk",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Belfast HSCNI Trust",
                Slug = "belfast-hscni",
                TenantType = TenantType.Hscni,
                OdsCode = "ZT001",
                Address = "10 Castle Street",
                City = "Belfast",
                Postcode = "BT1 1AA",
                PhoneNumber = "028 0000 0001",
                Email = "admin@belfast.hscni.net",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Smile Dental Care",
                Slug = "smile-dental",
                TenantType = TenantType.PrivatePractice,
                OdsCode = "V00001",
                Address = "25 Dental Row",
                City = "London",
                Postcode = "EC1A 1BB",
                PhoneNumber = "020 0000 0001",
                Email = "admin@smiledental.co.uk",
                IsActive = true
            }
        };

        context.Tenants.AddRange(tenants);
        await context.SaveChangesAsync();
        return tenants;
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, List<Tenant> tenants)
    {
        var nhsTenant = tenants.First(t => t.TenantType == TenantType.NhsEngland);
        var hscniTenant = tenants.First(t => t.TenantType == TenantType.Hscni);
        var dentalTenant = tenants.First(t => t.TenantType == TenantType.PrivatePractice);

        var users = new[]
        {
            (Email: "superadmin@patientcrm.nhs.uk", Password: "Admin@2024!", Role: "SuperAdmin", FirstName: "System", LastName: "Administrator", TenantId: nhsTenant.Id, GmcNumber: (string?)null),
            (Email: "dr.smith@greenfield.nhs.uk", Password: "Doctor@2024!", Role: "GP", FirstName: "James", LastName: "Smith", TenantId: nhsTenant.Id, GmcNumber: "1234567"),
            (Email: "dr.jones@greenfield.nhs.uk", Password: "Doctor@2024!", Role: "GP", FirstName: "Sarah", LastName: "Jones", TenantId: nhsTenant.Id, GmcNumber: "7654321"),
            (Email: "nurse.brown@greenfield.nhs.uk", Password: "Nurse@2024!", Role: "Nurse", FirstName: "Emma", LastName: "Brown", TenantId: nhsTenant.Id, GmcNumber: (string?)null),
            (Email: "reception@greenfield.nhs.uk", Password: "Reception@2024!", Role: "Receptionist", FirstName: "Lucy", LastName: "Wilson", TenantId: nhsTenant.Id, GmcNumber: (string?)null),
            (Email: "dr.o'neil@belfast.hscni.net", Password: "Doctor@2024!", Role: "GP", FirstName: "Patrick", LastName: "O'Neil", TenantId: hscniTenant.Id, GmcNumber: "9876543"),
            (Email: "dr.white@smiledental.co.uk", Password: "Dentist@2024!", Role: "Dentist", FirstName: "Charlotte", LastName: "White", TenantId: dentalTenant.Id, GmcNumber: (string?)null)
        };

        foreach (var (email, password, role, firstName, lastName, tenantId, gmcNumber) in users)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    TenantId = tenantId,
                    GmcNumber = gmcNumber,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, role);
            }
        }
    }

    private static async Task SeedPatientsAsync(ApplicationDbContext context, List<Tenant> tenants)
    {
        if (await context.Patients.AnyAsync())
            return;

        var nhsTenant = tenants.First(t => t.TenantType == TenantType.NhsEngland);
        var hscniTenant = tenants.First(t => t.TenantType == TenantType.Hscni);
        var dentalTenant = tenants.First(t => t.TenantType == TenantType.PrivatePractice);

        var patients = new List<Patient>
        {
            new()
            {
                Id = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = nhsTenant.Id,
                NhsNumber = "943 476 5925",
                FirstName = "William",
                LastName = "Taylor",
                DateOfBirth = new DateTime(1965, 3, 15),
                Gender = Gender.Male,
                BloodGroup = BloodGroup.APositive,
                AddressLine1 = "12 Oak Avenue",
                City = "Manchester",
                Postcode = "M2 3BC",
                Region = NhsRegion.England,
                PhoneNumber = "0161 000 1001",
                Email = "w.taylor@email.co.uk",
                RegisteredGpName = "Dr. James Smith",
                RegisteredGpPractice = "Greenfield Medical Practice",
                RegisteredGpOdsCode = "A81001",
                Status = PatientStatus.Active,
                ConsentToTreatment = true,
                ConsentToDataSharing = true,
                Allergies = "Penicillin",
                GpRecord = new GpRecord
                {
                    TenantId = nhsTenant.Id,
                    HasHypertension = true,
                    HasDiabetes = true,
                    DiabetesType = "Type 2",
                    HasFluVaccination = true,
                    LastFluVaccinationDate = new DateTime(2024, 10, 1),
                    BaselineBloodPressureSystolic = 140,
                    BaselineBloodPressureDiastolic = 90,
                    BaselineHba1c = 58.0m,
                    BaselineBmi = 28.5m
                }
            },
            new()
            {
                Id = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = nhsTenant.Id,
                NhsNumber = "943 476 5926",
                FirstName = "Margaret",
                LastName = "Hughes",
                DateOfBirth = new DateTime(1980, 7, 22),
                Gender = Gender.Female,
                BloodGroup = BloodGroup.OPositive,
                AddressLine1 = "45 Elm Street",
                City = "Salford",
                Postcode = "M5 4DE",
                Region = NhsRegion.England,
                PhoneNumber = "0161 000 1002",
                Email = "m.hughes@email.co.uk",
                RegisteredGpName = "Dr. Sarah Jones",
                RegisteredGpPractice = "Greenfield Medical Practice",
                RegisteredGpOdsCode = "A81001",
                Status = PatientStatus.Active,
                ConsentToTreatment = true,
                ConsentToDataSharing = true,
                GpRecord = new GpRecord
                {
                    TenantId = nhsTenant.Id,
                    HasAsthma = true,
                    LastCervicalScreening = new DateTime(2023, 5, 10)
                }
            },
            new()
            {
                Id = Guid.Parse("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = hscniTenant.Id,
                HscniNumber = "HC123456789",
                FirstName = "Seamus",
                LastName = "Murphy",
                DateOfBirth = new DateTime(1972, 11, 8),
                Gender = Gender.Male,
                BloodGroup = BloodGroup.BPositive,
                AddressLine1 = "78 Falls Road",
                City = "Belfast",
                Postcode = "BT12 6AH",
                Region = NhsRegion.NorthernIreland,
                PhoneNumber = "028 000 1003",
                Email = "s.murphy@email.co.uk",
                RegisteredGpName = "Dr. Patrick O'Neil",
                Status = PatientStatus.Active,
                ConsentToTreatment = true,
                ConsentToDataSharing = true,
                GpRecord = new GpRecord
                {
                    TenantId = hscniTenant.Id,
                    HasHeartDisease = true,
                    HasHypertension = true,
                    OnAtrialFibrillationRegister = true
                }
            },
            new()
            {
                Id = Guid.Parse("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = dentalTenant.Id,
                NhsNumber = "943 476 5928",
                FirstName = "Emily",
                LastName = "Clarke",
                DateOfBirth = new DateTime(1995, 2, 14),
                Gender = Gender.Female,
                BloodGroup = BloodGroup.ABNegative,
                AddressLine1 = "3 New Road",
                City = "London",
                Postcode = "EC1A 2CC",
                Region = NhsRegion.England,
                PhoneNumber = "020 000 1004",
                Email = "e.clarke@email.co.uk",
                Status = PatientStatus.Active,
                ConsentToTreatment = true,
                ConsentToDataSharing = true,
                DentalRecord = new DentalRecord
                {
                    TenantId = dentalTenant.Id,
                    LastExaminationDate = new DateTime(2024, 9, 15),
                    LastHygieneAppointment = new DateTime(2024, 9, 15),
                    LastXRayDate = new DateTime(2024, 3, 10),
                    OralHygieneSummary = "Good oral hygiene maintained",
                    PeriodontalStatus = "Healthy",
                    HasCrowns = true,
                    BasicPeriodontalExamination = "0,0,1,0,0,0"
                }
            }
        };

        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();

        // Add sample clinical notes
        var gpUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.smith@greenfield.nhs.uk");
        if (gpUser != null)
        {
            var notes = new List<ClinicalNote>
            {
                new()
                {
                    TenantId = nhsTenant.Id,
                    PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    NoteType = NoteType.Consultation,
                    Discipline = DisciplineType.GP,
                    Title = "Routine Diabetes Review",
                    Content = "Patient attended for routine 6-monthly diabetes review. HbA1c remains elevated at 58 mmol/mol. BP 138/88. Advised dietary changes and increased exercise. Metformin dose increased.",
                    Subjective = "Patient reports increased fatigue and thirst over last 4 weeks.",
                    Objective = "BP 138/88, Weight 87kg, BMI 28.5, HbA1c 58 mmol/mol",
                    Assessment = "Type 2 Diabetes - suboptimal control. Hypertension - borderline.",
                    Plan = "Increase Metformin to 1000mg BD. Repeat HbA1c in 3 months. Referral to diabetes nurse.",
                    SnomedCode = "44054006",
                    AuthorId = gpUser.Id,
                    AuthorName = gpUser.FullName,
                    NoteDate = DateTime.UtcNow.AddDays(-30),
                    RequiresFollowUp = true,
                    FollowUpDate = DateTime.UtcNow.AddMonths(3)
                },
                new()
                {
                    TenantId = nhsTenant.Id,
                    PatientId = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    NoteType = NoteType.Consultation,
                    Discipline = DisciplineType.GP,
                    Title = "Asthma Annual Review",
                    Content = "Annual asthma review. Good inhaler technique demonstrated. No exacerbations in past 12 months. Peak flow 420 L/min (predicted 480 L/min).",
                    Subjective = "Patient feels well controlled. Using reliever inhaler less than twice a week.",
                    Objective = "Chest clear. Peak flow 420 L/min. O2 sats 99%.",
                    Assessment = "Mild persistent asthma - well controlled.",
                    Plan = "Continue current inhalers. Step down to low-dose ICS if remains well controlled. Flu vaccination offered.",
                    SnomedCode = "195967001",
                    AuthorId = gpUser.Id,
                    AuthorName = gpUser.FullName,
                    NoteDate = DateTime.UtcNow.AddDays(-14)
                }
            };

            context.ClinicalNotes.AddRange(notes);
            await context.SaveChangesAsync();
        }
    }
}
