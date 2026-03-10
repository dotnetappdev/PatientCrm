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

        await SeedRolesAsync(roleManager);

        var tenants = await SeedTenantsAsync(context);

        await SeedUsersAsync(userManager, tenants);

        await SeedPatientsAsync(context, tenants);

        await SeedPatientPortalUsersAsync(context, userManager, tenants);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roles = ["SuperAdmin", "TenantAdmin", "GP", "Dentist", "Consultant", "Nurse", "Receptionist", "ReadOnly", "Patient"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role, Description = $"{role} role", NormalizedName = role.ToUpper() });
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
                ClientType = ClientType.GpPractice,
                OdsCode = "A81001",
                Address = "1 High Street",
                City = "Manchester",
                Postcode = "M1 1AA",
                PhoneNumber = "0161 000 0001",
                Email = "admin@greenfield.nhs.uk",
                Website = "https://www.greenfield.nhs.uk",
                ContactName = "Practice Manager",
                Notes = "NHS GP Practice serving Central Manchester. CQC Outstanding rating.",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Belfast HSCNI Trust",
                Slug = "belfast-hscni",
                TenantType = TenantType.Hscni,
                ClientType = ClientType.HospitalConsulting,
                OdsCode = "ZT001",
                Address = "10 Castle Street",
                City = "Belfast",
                Postcode = "BT1 1AA",
                PhoneNumber = "028 0000 0001",
                Email = "admin@belfast.hscni.net",
                Website = "https://www.belfasttrust.hscni.net",
                ContactName = "Trust Administrator",
                Notes = "HSCNI Hospital Trust providing secondary care services across Belfast.",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Smile Dental Care",
                Slug = "smile-dental",
                TenantType = TenantType.PrivatePractice,
                ClientType = ClientType.DentalPractice,
                OdsCode = "V00001",
                Address = "25 Dental Row",
                City = "London",
                Postcode = "EC1A 1BB",
                PhoneNumber = "020 0000 0001",
                Email = "admin@smiledental.co.uk",
                Website = "https://www.smiledental.co.uk",
                ContactName = "Practice Owner",
                Notes = "Private dental practice in Central London. Full range of NHS and private treatments.",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Riverside Health Centre",
                Slug = "riverside-health",
                TenantType = TenantType.NhsEngland,
                ClientType = ClientType.HealthCentre,
                OdsCode = "B12345",
                Address = "100 Riverside Way",
                City = "Leeds",
                Postcode = "LS1 3AB",
                PhoneNumber = "0113 000 0002",
                Email = "admin@riverside.nhs.uk",
                ContactName = "Centre Manager",
                Notes = "NHS Health Centre offering GP, nursing and allied health services.",
                IsActive = true
            }
        };

        context.Tenants.AddRange(tenants);
        await context.SaveChangesAsync();
        return tenants;
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, List<Tenant> tenants)
    {
        var nhsTenant = tenants.First(t => t.TenantType == TenantType.NhsEngland && t.ClientType == ClientType.GpPractice);
        var hscniTenant = tenants.First(t => t.TenantType == TenantType.Hscni);
        var dentalTenant = tenants.First(t => t.ClientType == ClientType.DentalPractice);
        var healthCentre = tenants.First(t => t.ClientType == ClientType.HealthCentre);

        var users = new[]
        {
            (Email: "superadmin@patientcrm.nhs.uk", Password: "Admin@2024!", Role: "SuperAdmin",
             FirstName: "System", LastName: "Administrator", Title: "Mr", TenantId: nhsTenant.Id,
             GmcNumber: (string?)null, GdcNumber: (string?)null),

            (Email: "dr.smith@greenfield.nhs.uk", Password: "Doctor@2024!", Role: "GP",
             FirstName: "James", LastName: "Smith", Title: "Dr", TenantId: nhsTenant.Id,
             GmcNumber: "1234567", GdcNumber: (string?)null),

            (Email: "dr.jones@greenfield.nhs.uk", Password: "Doctor@2024!", Role: "GP",
             FirstName: "Sarah", LastName: "Jones", Title: "Dr", TenantId: nhsTenant.Id,
             GmcNumber: "7654321", GdcNumber: (string?)null),

            (Email: "nurse.brown@greenfield.nhs.uk", Password: "Nurse@2024!", Role: "Nurse",
             FirstName: "Emma", LastName: "Brown", Title: "Ms", TenantId: nhsTenant.Id,
             GmcNumber: (string?)null, GdcNumber: (string?)null),

            (Email: "reception@greenfield.nhs.uk", Password: "Reception@2024!", Role: "Receptionist",
             FirstName: "Lucy", LastName: "Wilson", Title: "Ms", TenantId: nhsTenant.Id,
             GmcNumber: (string?)null, GdcNumber: (string?)null),

            (Email: "admin@greenfield.nhs.uk", Password: "Admin@2024!", Role: "TenantAdmin",
             FirstName: "Practice", LastName: "Manager", Title: "Ms", TenantId: nhsTenant.Id,
             GmcNumber: (string?)null, GdcNumber: (string?)null),

            (Email: "dr.oneil@belfast.hscni.net", Password: "Doctor@2024!", Role: "GP",
             FirstName: "Patrick", LastName: "O'Neil", Title: "Dr", TenantId: hscniTenant.Id,
             GmcNumber: "9876543", GdcNumber: (string?)null),

            (Email: "consultant.mclaughlin@belfast.hscni.net", Password: "Doctor@2024!", Role: "Consultant",
             FirstName: "Aoife", LastName: "McLaughlin", Title: "Dr", TenantId: hscniTenant.Id,
             GmcNumber: "8765432", GdcNumber: (string?)null),

            (Email: "dr.white@smiledental.co.uk", Password: "Dentist@2024!", Role: "Dentist",
             FirstName: "Charlotte", LastName: "White", Title: "Dr", TenantId: dentalTenant.Id,
             GmcNumber: (string?)null, GdcNumber: "123456"),

            (Email: "dr.patel@riverside.nhs.uk", Password: "Doctor@2024!", Role: "GP",
             FirstName: "Priya", LastName: "Patel", Title: "Dr", TenantId: healthCentre.Id,
             GmcNumber: "6543210", GdcNumber: (string?)null)
        };

        foreach (var u in users)
        {
            if (await userManager.FindByEmailAsync(u.Email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = u.Email, Email = u.Email,
                    FirstName = u.FirstName, LastName = u.LastName, Title = u.Title,
                    TenantId = u.TenantId, GmcNumber = u.GmcNumber, GdcNumber = u.GdcNumber,
                    EmailConfirmed = true, IsActive = true
                };
                var result = await userManager.CreateAsync(user, u.Password);
                if (result.Succeeded) await userManager.AddToRoleAsync(user, u.Role);
            }
        }
    }

    private static async Task SeedPatientsAsync(ApplicationDbContext context, List<Tenant> tenants)
    {
        if (await context.Patients.AnyAsync())
            return;

        var nhsTenant = tenants.First(t => t.TenantType == TenantType.NhsEngland && t.ClientType == ClientType.GpPractice);
        var hscniTenant = tenants.First(t => t.TenantType == TenantType.Hscni);
        var dentalTenant = tenants.First(t => t.ClientType == ClientType.DentalPractice);
        var healthCentre = tenants.First(t => t.ClientType == ClientType.HealthCentre);

        var patients = new List<Patient>
        {
            // NHS England GP Patients
            new()
            {
                Id = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = nhsTenant.Id,
                NhsNumber = "943 476 5925",
                FirstName = "William", LastName = "Taylor",
                DateOfBirth = new DateTime(1958, 3, 15),
                Gender = Gender.Male, BloodGroup = BloodGroup.APositive,
                AddressLine1 = "12 Oak Avenue", City = "Manchester", Postcode = "M2 3BC",
                Region = NhsRegion.England, Country = "United Kingdom",
                PhoneNumber = "0161 000 1001", MobileNumber = "07700 900001",
                Email = "w.taylor@email.co.uk",
                RegisteredGpName = "Dr. James Smith", RegisteredGpPractice = "Greenfield Medical Practice",
                RegisteredGpOdsCode = "A81001", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "Penicillin (causes rash)", CurrentMedications = "Metformin 1000mg BD, Lisinopril 5mg OD, Atorvastatin 40mg ON",
                HasSmokingHistory = true, HasAlcoholHistory = false,
                EmergencyContactName = "Susan Taylor", EmergencyContactRelationship = "Spouse",
                EmergencyContactPhone = "07700 900002",
                GpRecord = new GpRecord
                {
                    TenantId = nhsTenant.Id,
                    HasHypertension = true, HasDiabetes = true, DiabetesType = "Type 2",
                    
                    HasFluVaccination = true, LastFluVaccinationDate = new DateTime(2024, 10, 1),
                    BaselineBloodPressureSystolic = 142, BaselineBloodPressureDiastolic = 88,
                    BaselineHba1c = 58.0m, BaselineBmi = 29.2m, BaselineCholesterol = 5.8m
                }
            },
            new()
            {
                Id = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = nhsTenant.Id,
                NhsNumber = "943 476 5926",
                FirstName = "Margaret", LastName = "Hughes",
                DateOfBirth = new DateTime(1982, 7, 22),
                Gender = Gender.Female, BloodGroup = BloodGroup.OPositive,
                AddressLine1 = "45 Elm Street", City = "Salford", Postcode = "M5 4DE",
                Region = NhsRegion.England, Country = "United Kingdom",
                PhoneNumber = "0161 000 1002", MobileNumber = "07700 900003",
                Email = "m.hughes@email.co.uk",
                RegisteredGpName = "Dr. Sarah Jones", RegisteredGpPractice = "Greenfield Medical Practice",
                RegisteredGpOdsCode = "A81001", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "None known", CurrentMedications = "Salbutamol 100mcg inhaler PRN, Beclometasone 200mcg BD",
                EmergencyContactName = "David Hughes", EmergencyContactRelationship = "Husband",
                EmergencyContactPhone = "07700 900004",
                GpRecord = new GpRecord
                {
                    TenantId = nhsTenant.Id,
                    HasAsthma = true, LastCervicalScreening = new DateTime(2023, 5, 10),
                     BaselineBmi = 24.1m
                }
            },
            new()
            {
                Id = Guid.Parse("aaaaaaa5-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = nhsTenant.Id,
                NhsNumber = "943 476 5929",
                FirstName = "Robert", LastName = "Blackwood",
                DateOfBirth = new DateTime(1945, 11, 30),
                Gender = Gender.Male, BloodGroup = BloodGroup.BPositive,
                AddressLine1 = "88 Maple Drive", City = "Manchester", Postcode = "M3 7EF",
                Region = NhsRegion.England, Country = "United Kingdom",
                PhoneNumber = "0161 000 1005", MobileNumber = "07700 900009",
                Email = "r.blackwood@email.co.uk",
                RegisteredGpName = "Dr. James Smith", RegisteredGpPractice = "Greenfield Medical Practice",
                RegisteredGpOdsCode = "A81001", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "Aspirin (gastrointestinal bleeding)", CurrentMedications = "Bisoprolol 2.5mg OD, Ramipril 5mg OD, Furosemide 40mg OD, Warfarin 3mg OD",
                HasSmokingHistory = true,
                EmergencyContactName = "Mary Blackwood", EmergencyContactRelationship = "Wife",
                EmergencyContactPhone = "07700 900010",
                GpRecord = new GpRecord
                {
                    TenantId = nhsTenant.Id,
                    HasHeartDisease = true, HasHypertension = true,
                    OnAtrialFibrillationRegister = true,
                    HasFluVaccination = true, LastFluVaccinationDate = new DateTime(2024, 10, 5),
                    BaselineBloodPressureSystolic = 128, BaselineBloodPressureDiastolic = 76,
                    BaselineBmi = 26.4m, BaselineCholesterol = 4.2m
                }
            },
            new()
            {
                Id = Guid.Parse("aaaaaaa6-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = nhsTenant.Id,
                NhsNumber = "943 476 5930",
                FirstName = "Fatima", LastName = "Al-Rashid",
                DateOfBirth = new DateTime(1990, 4, 18),
                Gender = Gender.Female, BloodGroup = BloodGroup.ABPositive,
                AddressLine1 = "15 Birchwood Close", City = "Manchester", Postcode = "M8 2GH",
                Region = NhsRegion.England, Country = "United Kingdom",
                PhoneNumber = "0161 000 1006", MobileNumber = "07700 900011",
                Email = "f.alrashid@email.co.uk",
                PreferredLanguage = "Arabic", RequiresInterpreter = false,
                RegisteredGpName = "Dr. Sarah Jones", RegisteredGpPractice = "Greenfield Medical Practice",
                RegisteredGpOdsCode = "A81001", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "None known", CurrentMedications = "Levothyroxine 75mcg OD",
                EmergencyContactName = "Ahmed Al-Rashid", EmergencyContactRelationship = "Husband",
                EmergencyContactPhone = "07700 900012",
                GpRecord = new GpRecord
                {
                    TenantId = nhsTenant.Id,
                    HasMentalHealthCondition = false, LastCervicalScreening = new DateTime(2024, 2, 20),
                    BaselineBmi = 22.8m
                }
            },
            new()
            {
                Id = Guid.Parse("aaaaaaa7-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = nhsTenant.Id,
                NhsNumber = "943 476 5931",
                FirstName = "Thomas", LastName = "O'Brien",
                DateOfBirth = new DateTime(1968, 9, 5),
                Gender = Gender.Male, BloodGroup = BloodGroup.ONegative,
                AddressLine1 = "27 Victoria Street", City = "Manchester", Postcode = "M1 5HJ",
                Region = NhsRegion.England, Country = "United Kingdom",
                PhoneNumber = "0161 000 1007", MobileNumber = "07700 900013",
                Email = "t.obrien@email.co.uk",
                RegisteredGpName = "Dr. James Smith", RegisteredGpPractice = "Greenfield Medical Practice",
                RegisteredGpOdsCode = "A81001", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "Codeine (nausea/vomiting)", CurrentMedications = "Omeprazole 20mg OD, Sertraline 100mg OD",
                HasAlcoholHistory = true,
                EmergencyContactName = "Claire O'Brien", EmergencyContactRelationship = "Daughter",
                EmergencyContactPhone = "07700 900014",
                GpRecord = new GpRecord
                {
                    TenantId = nhsTenant.Id,
                    HasMentalHealthCondition = true, BaselineBmi = 27.3m
                }
            },

            // HSCNI Patients
            new()
            {
                Id = Guid.Parse("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = hscniTenant.Id,
                HscniNumber = "HC123456789",
                FirstName = "Seamus", LastName = "Murphy",
                DateOfBirth = new DateTime(1972, 11, 8),
                Gender = Gender.Male, BloodGroup = BloodGroup.BPositive,
                AddressLine1 = "78 Falls Road", City = "Belfast", Postcode = "BT12 6AH",
                Region = NhsRegion.NorthernIreland, Country = "United Kingdom",
                PhoneNumber = "028 000 1003", MobileNumber = "07700 900005",
                Email = "s.murphy@email.co.uk",
                RegisteredGpName = "Dr. Patrick O'Neil", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "Statin drugs (myopathy)", CurrentMedications = "Bisoprolol 5mg OD, Apixaban 5mg BD, Ramipril 10mg OD",
                HasSmokingHistory = true,
                EmergencyContactName = "Brigid Murphy", EmergencyContactRelationship = "Wife",
                EmergencyContactPhone = "07700 900006",
                GpRecord = new GpRecord
                {
                    TenantId = hscniTenant.Id,
                    HasHeartDisease = true, HasHypertension = true, OnAtrialFibrillationRegister = true,
                    BaselineBloodPressureSystolic = 132, BaselineBloodPressureDiastolic = 80,
                    BaselineBmi = 31.5m, BaselineCholesterol = 5.1m
                }
            },
            new()
            {
                Id = Guid.Parse("aaaaaaa8-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = hscniTenant.Id,
                HscniNumber = "HC234567890",
                FirstName = "Siobhan", LastName = "Doherty",
                DateOfBirth = new DateTime(1985, 6, 14),
                Gender = Gender.Female, BloodGroup = BloodGroup.APositive,
                AddressLine1 = "12 Antrim Road", City = "Belfast", Postcode = "BT15 2AA",
                Region = NhsRegion.NorthernIreland, Country = "United Kingdom",
                PhoneNumber = "028 000 1008", MobileNumber = "07700 900015",
                Email = "s.doherty@email.co.uk",
                RegisteredGpName = "Dr. Patrick O'Neil", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "None known", CurrentMedications = "Insulin Glargine 24 units ON, Insulin Aspart PRN with meals",
                EmergencyContactName = "Colm Doherty", EmergencyContactRelationship = "Brother",
                EmergencyContactPhone = "07700 900016",
                GpRecord = new GpRecord
                {
                    TenantId = hscniTenant.Id,
                    HasDiabetes = true, DiabetesType = "Type 1",
                    BaselineHba1c = 62.0m, BaselineBmi = 23.7m
                }
            },

            // Dental Patients
            new()
            {
                Id = Guid.Parse("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = dentalTenant.Id,
                NhsNumber = "943 476 5928",
                FirstName = "Emily", LastName = "Clarke",
                DateOfBirth = new DateTime(1995, 2, 14),
                Gender = Gender.Female, BloodGroup = BloodGroup.ABNegative,
                AddressLine1 = "3 New Road", City = "London", Postcode = "EC1A 2CC",
                Region = NhsRegion.England, Country = "United Kingdom",
                PhoneNumber = "020 000 1004", MobileNumber = "07700 900007",
                Email = "e.clarke@email.co.uk",
                RegisteredGpName = "Dr. Local GP", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "Latex (contact urticaria)", CurrentMedications = "None",
                EmergencyContactName = "John Clarke", EmergencyContactRelationship = "Father",
                EmergencyContactPhone = "07700 900008",
                DentalRecord = new DentalRecord
                {
                    TenantId = dentalTenant.Id,
                    LastExaminationDate = new DateTime(2024, 9, 15),
                    LastHygieneAppointment = new DateTime(2024, 9, 15),
                    LastXRayDate = new DateTime(2024, 3, 10),
                    OralHygieneSummary = "Good oral hygiene. Attends regularly for check-ups.",
                    PeriodontalStatus = "Healthy gingiva, no attachment loss",
                    HasCrowns = true, 
                    BasicPeriodontalExamination = "0,0,1,0,0,0"
                }
            },
            new()
            {
                Id = Guid.Parse("aaaaaaa9-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = dentalTenant.Id,
                NhsNumber = "943 476 5932",
                FirstName = "George", LastName = "Davies",
                DateOfBirth = new DateTime(1960, 8, 20),
                Gender = Gender.Male, BloodGroup = BloodGroup.OPositive,
                AddressLine1 = "50 King Street", City = "London", Postcode = "WC2B 5AA",
                Region = NhsRegion.England, Country = "United Kingdom",
                PhoneNumber = "020 000 1009", MobileNumber = "07700 900017",
                Email = "g.davies@email.co.uk",
                RegisteredGpName = "Dr. Local GP", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "Ibuprofen (asthma exacerbation)", CurrentMedications = "Salbutamol inhaler PRN",
                DentalRecord = new DentalRecord
                {
                    TenantId = dentalTenant.Id,
                    LastExaminationDate = new DateTime(2024, 11, 5),
                    LastXRayDate = new DateTime(2024, 11, 5),
                    OralHygieneSummary = "Moderate plaque levels. Requires improved home care.",
                    PeriodontalStatus = "Moderate periodontitis, pocketing 4-5mm posterior",
                    HasDentures = true, HasImplants = true, BasicPeriodontalExamination = "1,2,3,2,1,0"
                }
            },

            // Health Centre Patient
            new()
            {
                Id = Guid.Parse("aaaaaa10-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = healthCentre.Id,
                NhsNumber = "943 476 5933",
                FirstName = "Preethi", LastName = "Sharma",
                DateOfBirth = new DateTime(1975, 12, 3),
                Gender = Gender.Female, BloodGroup = BloodGroup.BPositive,
                AddressLine1 = "22 Riverside Terrace", City = "Leeds", Postcode = "LS2 7PQ",
                Region = NhsRegion.England, Country = "United Kingdom",
                PhoneNumber = "0113 000 1010", MobileNumber = "07700 900018",
                Email = "p.sharma@email.co.uk",
                RegisteredGpName = "Dr. Priya Patel", Status = PatientStatus.Active,
                ConsentToTreatment = true, ConsentToDataSharing = true,
                Allergies = "None known", CurrentMedications = "Methotrexate 15mg weekly, Folic acid 5mg OD (6 days/week)",
                EmergencyContactName = "Raj Sharma", EmergencyContactRelationship = "Husband",
                EmergencyContactPhone = "07700 900019",
                GpRecord = new GpRecord
                {
                    TenantId = healthCentre.Id,
                    HasMentalHealthCondition = false, BaselineBmi = 26.0m
                }
            }
        };

        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();

        // Seed clinical notes, prescriptions, appointments and alerts
        await SeedClinicalNotesAsync(context, nhsTenant, hscniTenant);
        await SeedAppointmentsAsync(context, nhsTenant, hscniTenant, dentalTenant, healthCentre);
        await SeedPrescriptionsAsync(context, nhsTenant, hscniTenant);
        await SeedAlertsAsync(context, nhsTenant);
        await SeedDepartmentsAsync(context, hscniTenant);
    }

    private static async Task SeedClinicalNotesAsync(ApplicationDbContext context, Tenant nhsTenant, Tenant hscniTenant)
    {
        var gpUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.smith@greenfield.nhs.uk");
        var gpUser2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.jones@greenfield.nhs.uk");
        var hscniGp = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.oneil@belfast.hscni.net");

        if (gpUser == null) return;

        var notes = new List<ClinicalNote>
        {
            // William Taylor - Diabetes Review
            new()
            {
                TenantId = nhsTenant.Id,
                PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                NoteType = NoteType.Consultation, Discipline = DisciplineType.GP,
                Title = "Routine Diabetes Review",
                Content = "Patient attended for routine 6-monthly diabetes review. HbA1c remains elevated at 58 mmol/mol. BP 142/88. Weight 87kg, BMI 29.2. Discussed dietary changes, increased physical activity target to 150 mins/week. Metformin dose increased to 1000mg BD. Reinforced importance of foot care and annual diabetic eye screening.",
                Subjective = "Patient reports increased fatigue and excessive thirst over last 4 weeks. Polyuria noted. Compliance with medication confirmed.",
                Objective = "BP 142/88, Weight 87kg, BMI 29.2, HbA1c 58 mmol/mol, Total cholesterol 5.8, eGFR 72",
                Assessment = "Type 2 Diabetes mellitus (E11) - suboptimal control. Hypertension (I10) - borderline control. Dyslipidaemia.",
                Plan = "1. Increase Metformin to 1000mg BD. 2. Repeat HbA1c in 3 months. 3. Refer to diabetes specialist nurse. 4. Arrange dietitian referral. 5. Diabetic retinal screening reminder sent.",
                SnomedCode = "44054006",
                AuthorId = gpUser.Id, AuthorName = gpUser.FullName,
                NoteDate = DateTime.UtcNow.AddDays(-30),
                RequiresFollowUp = true, FollowUpDate = DateTime.UtcNow.AddMonths(3)
            },
            new()
            {
                TenantId = nhsTenant.Id,
                PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                NoteType = NoteType.FollowUp, Discipline = DisciplineType.GP,
                Title = "Hypertension Follow-up",
                Content = "Follow-up for hypertension. BP improved to 130/82 following dose titration. Patient tolerating Lisinopril well. No side effects. Continue current management.",
                Subjective = "No symptoms. Patient reports BP readings at home 128-136/78-84.",
                Objective = "BP 130/82. Pulse 68 regular. No peripheral oedema.",
                Assessment = "Hypertension - improving control on Lisinopril 5mg.",
                Plan = "Continue Lisinopril 5mg OD. Recheck BP in 3 months. Annual bloods due next month.",
                SnomedCode = "38341003",
                AuthorId = gpUser.Id, AuthorName = gpUser.FullName,
                NoteDate = DateTime.UtcNow.AddDays(-10)
            },

            // Margaret Hughes - Asthma Review
            new()
            {
                TenantId = nhsTenant.Id,
                PatientId = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                NoteType = NoteType.Consultation, Discipline = DisciplineType.GP,
                Title = "Annual Asthma Review",
                Content = "Annual asthma review. Patient well-controlled. No exacerbations or acute attacks in past 12 months. Reliever use less than twice per week. Good inhaler technique demonstrated. Asthma control test score 22/25.",
                Subjective = "Patient feels well controlled. Occasional wheeze with exercise. No nocturnal symptoms.",
                Objective = "Chest clear to auscultation. Peak flow 420 L/min (predicted 480). O2 sats 99% on air.",
                Assessment = "Mild persistent asthma (J45.1) - well controlled on SABA/ICS regime.",
                Plan = "Continue Beclometasone 200mcg BD and Salbutamol PRN. Consider step-down if remains controlled at 6-month review. Flu vaccination offered and administered.",
                SnomedCode = "195967001",
                AuthorId = gpUser2 != null ? gpUser2.Id : gpUser.Id,
                AuthorName = gpUser2 != null ? gpUser2.FullName : gpUser.FullName,
                NoteDate = DateTime.UtcNow.AddDays(-14)
            },

            // Robert Blackwood - Heart Failure
            new()
            {
                TenantId = nhsTenant.Id,
                PatientId = Guid.Parse("aaaaaaa5-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                NoteType = NoteType.Consultation, Discipline = DisciplineType.GP,
                Title = "Heart Failure / Atrial Fibrillation Review",
                Content = "Patient attended for 3-monthly heart failure review. Symptoms stable. No increased breathlessness or ankle swelling. INR therapeutic on Warfarin. NYHA Class II. Cardiology follow-up appointment booked.",
                Subjective = "Patient stable. Mild dyspnoea on exertion (stairs). No orthopnoea or PND. Peripheral oedema resolved.",
                Objective = "Pulse 72 irregular. BP 128/76. JVP not elevated. Fine basal crepitations. O2 sats 97%. Weight 74kg (unchanged).",
                Assessment = "Chronic heart failure (I50.0), atrial fibrillation (I48) - stable. Anticoagulated on Warfarin.",
                Plan = "Continue current medications. INR check in 4 weeks. Cardiology review in 6 months. Annual flu and pneumococcal vaccination up to date.",
                SnomedCode = "84114007",
                AuthorId = gpUser.Id, AuthorName = gpUser.FullName,
                NoteDate = DateTime.UtcNow.AddDays(-7),
                RequiresFollowUp = true, FollowUpDate = DateTime.UtcNow.AddMonths(3)
            },

            // Seamus Murphy - HSCNI
            new()
            {
                TenantId = hscniTenant.Id,
                PatientId = Guid.Parse("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                NoteType = NoteType.Consultation, Discipline = DisciplineType.GP,
                Title = "Atrial Fibrillation and Hypertension Review",
                Content = "Patient reviewed for AF and hypertension management. Palpitations well controlled on Bisoprolol. BP within target range. On Apixaban for stroke prevention, no bleeding concerns reported. CHA2DS2-VASc score 3.",
                Subjective = "Patient reports occasional mild palpitations but much improved. No dizziness or presyncope.",
                Objective = "HR 78 irregular. BP 132/80. No signs of fluid overload. Weight 98kg.",
                Assessment = "Persistent atrial fibrillation (I48.2), hypertension (I10) - well controlled.",
                Plan = "Continue Bisoprolol 5mg OD, Apixaban 5mg BD, Ramipril 10mg OD. Annual CHADS2 review. Echo booked.",
                SnomedCode = "49436004",
                AuthorId = hscniGp?.Id ?? gpUser.Id,
                AuthorName = hscniGp?.FullName ?? gpUser.FullName,
                NoteDate = DateTime.UtcNow.AddDays(-21)
            }
        };

        context.ClinicalNotes.AddRange(notes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAppointmentsAsync(ApplicationDbContext context, Tenant nhsTenant, Tenant hscniTenant, Tenant dentalTenant, Tenant healthCentre)
    {
        var gpUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.smith@greenfield.nhs.uk");
        var gpUser2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.jones@greenfield.nhs.uk");
        var dentist = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.white@smiledental.co.uk");
        var hscniGp = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.oneil@belfast.hscni.net");
        var healthGp = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.patel@riverside.nhs.uk");

        if (gpUser == null) return;

        var today = DateTime.Today;
        var appointments = new List<Appointment>
        {
            // Today's appointments - NHS GP
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = gpUser.Id, Title = "Diabetes 3-Month Review",
                StartTime = today.AddHours(9), EndTime = today.AddHours(9).AddMinutes(20),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Confirmed,
                Discipline = DisciplineType.GP, Notes = "Check HbA1c results. Review Metformin dosing."
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = gpUser2 != null ? gpUser2.Id : gpUser.Id,
                Title = "Asthma Follow-up", StartTime = today.AddHours(9.5), EndTime = today.AddHours(9.5).AddMinutes(15),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Scheduled,
                Discipline = DisciplineType.GP, Notes = "Post annual review check. Peak flow diary review."
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa7-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = gpUser.Id, Title = "Mental Health Check-in",
                StartTime = today.AddHours(10), EndTime = today.AddHours(10).AddMinutes(20),
                AppointmentType = AppointmentType.Telephone, Status = AppointmentStatus.Confirmed,
                Discipline = DisciplineType.GP, Notes = "PHQ-9 review. Sertraline 8-week follow-up."
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa5-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = gpUser.Id, Title = "INR Blood Test Review",
                StartTime = today.AddHours(11), EndTime = today.AddHours(11).AddMinutes(10),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Scheduled,
                Discipline = DisciplineType.GP, Notes = "Check INR result. Adjust Warfarin if needed."
            },

            // Future appointments
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa6-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = gpUser2 != null ? gpUser2.Id : gpUser.Id,
                Title = "Thyroid Function Review", StartTime = today.AddDays(3).AddHours(14),
                EndTime = today.AddDays(3).AddHours(14).AddMinutes(15),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Scheduled,
                Discipline = DisciplineType.GP, Notes = "Annual TFT check. Review Levothyroxine dose."
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = gpUser.Id, Title = "Annual Review - Bloods",
                StartTime = today.AddDays(7).AddHours(9),
                EndTime = today.AddDays(7).AddHours(9).AddMinutes(20),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Scheduled,
                Discipline = DisciplineType.GP, Notes = "Annual blood tests including renal function, LFT, HbA1c, lipids."
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = gpUser2 != null ? gpUser2.Id : gpUser.Id,
                Title = "Contraception Consultation", StartTime = today.AddDays(5).AddHours(15),
                EndTime = today.AddDays(5).AddHours(15).AddMinutes(20),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Scheduled,
                Discipline = DisciplineType.GP
            },

            // HSCNI Appointments
            new() {
                TenantId = hscniTenant.Id, PatientId = Guid.Parse("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = hscniGp?.Id ?? gpUser.Id, Title = "AF Clinic",
                StartTime = today.AddDays(2).AddHours(10), EndTime = today.AddDays(2).AddHours(10).AddMinutes(30),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Confirmed,
                Discipline = DisciplineType.Consultant, Notes = "Echocardiogram results review."
            },
            new() {
                TenantId = hscniTenant.Id, PatientId = Guid.Parse("aaaaaaa8-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = hscniGp?.Id ?? gpUser.Id, Title = "Diabetes Insulin Adjustment",
                StartTime = today.AddDays(1).AddHours(11), EndTime = today.AddDays(1).AddHours(11).AddMinutes(20),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Confirmed,
                Discipline = DisciplineType.GP, Notes = "Continuous glucose monitor download review. Basal insulin titration."
            },

            // Dental Appointments
            new() {
                TenantId = dentalTenant.Id, PatientId = Guid.Parse("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = dentist?.Id ?? gpUser.Id, Title = "6-Month Dental Check-up",
                StartTime = today.AddDays(4).AddHours(13), EndTime = today.AddDays(4).AddHours(13).AddMinutes(30),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Scheduled,
                Discipline = DisciplineType.Dentist, Notes = "Routine examination and scale and polish."
            },
            new() {
                TenantId = dentalTenant.Id, PatientId = Guid.Parse("aaaaaaa9-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = dentist?.Id ?? gpUser.Id, Title = "Periodontal Treatment",
                StartTime = today.AddDays(1).AddHours(14), EndTime = today.AddDays(1).AddHours(14).AddMinutes(60),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Confirmed,
                Discipline = DisciplineType.Dentist, Notes = "Deep scaling and root planing - quadrant 2."
            },

            // Past completed appointments
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = gpUser.Id, Title = "Diabetes 6-Month Review",
                StartTime = today.AddDays(-30).AddHours(9), EndTime = today.AddDays(-30).AddHours(9).AddMinutes(20),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Completed,
                Discipline = DisciplineType.GP, Notes = "HbA1c 58, Metformin increased."
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = gpUser2 != null ? gpUser2.Id : gpUser.Id,
                Title = "Asthma Annual Review",
                StartTime = today.AddDays(-14).AddHours(10), EndTime = today.AddDays(-14).AddHours(10).AddMinutes(15),
                AppointmentType = AppointmentType.InPerson, Status = AppointmentStatus.Completed,
                Discipline = DisciplineType.GP
            }
        };

        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPrescriptionsAsync(ApplicationDbContext context, Tenant nhsTenant, Tenant hscniTenant)
    {
        var gpUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.smith@greenfield.nhs.uk");
        var gpUser2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.jones@greenfield.nhs.uk");
        var hscniGp = await context.Users.FirstOrDefaultAsync(u => u.Email == "dr.oneil@belfast.hscni.net");

        if (gpUser == null) return;

        var prescriptions = new List<Prescription>
        {
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = gpUser.Id, MedicationName = "Metformin 1000mg tablets",
                Route = "Oral",
                Dosage = "1 tablet", Frequency = "Twice daily",
                QuantityIssued = 30, Instructions = "Take with or after meals. Swallow whole with water.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-30),
                SnomedCode = "372814007"
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = gpUser.Id, MedicationName = "Lisinopril 5mg tablets",
                Route = "Oral",
                Dosage = "1 tablet", Frequency = "Once daily",
                QuantityIssued = 30, Instructions = "Take in the morning. Monitor for dry cough.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-60),
                SnomedCode = "386873009"
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = gpUser.Id, MedicationName = "Atorvastatin 40mg tablets",
                Route = "Oral",
                Dosage = "1 tablet", Frequency = "Once nightly",
                QuantityIssued = 30, Instructions = "Take at night. Avoid grapefruit juice.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-60),
                SnomedCode = "372912004"
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = gpUser2 != null ? gpUser2.Id : gpUser.Id,
                MedicationName = "Beclometasone dipropionate 200mcg inhaler",
                Route = "Inhalation",
                Dosage = "2 puffs", Frequency = "Twice daily",
                QuantityIssued = 1, Instructions = "Use as preventer inhaler. Rinse mouth after use.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-90),
                SnomedCode = "372490001"
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = gpUser2 != null ? gpUser2.Id : gpUser.Id,
                MedicationName = "Salbutamol 100mcg inhaler",
                Route = "Inhalation",
                Dosage = "1-2 puffs", Frequency = "As required",
                QuantityIssued = 1, Instructions = "Use as reliever when breathless or wheezy. If using more than twice a week, contact surgery.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-90),
                SnomedCode = "372832002"
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa5-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = gpUser.Id, MedicationName = "Warfarin sodium 3mg tablets",
                Route = "Oral",
                Dosage = "As directed by INR", Frequency = "Once daily",
                QuantityIssued = 30, Instructions = "Dose adjusted according to INR. Keep INR target 2-3. Carry anticoagulant card.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-180),
                SnomedCode = "372756006"
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa5-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = gpUser.Id, MedicationName = "Bisoprolol fumarate 2.5mg tablets",
                Route = "Oral",
                Dosage = "1 tablet", Frequency = "Once daily",
                QuantityIssued = 30, Instructions = "Do not stop suddenly. Report any breathlessness.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-180),
                SnomedCode = "318475009"
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa7-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = gpUser.Id, MedicationName = "Sertraline 100mg tablets",
                Route = "Oral",
                Dosage = "1 tablet", Frequency = "Once daily in the morning",
                QuantityIssued = 30, Instructions = "Take in the morning with food. Allow 4-6 weeks for full effect.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-56),
                SnomedCode = "372533001"
            },
            new() {
                TenantId = hscniTenant.Id, PatientId = Guid.Parse("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = hscniGp?.Id ?? gpUser.Id,
                MedicationName = "Apixaban 5mg tablets",
                Route = "Oral",
                Dosage = "1 tablet", Frequency = "Twice daily",
                QuantityIssued = 30, Instructions = "For stroke prevention in AF. Do not stop without medical advice.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-120),
                SnomedCode = "700408004"
            },
            new() {
                TenantId = hscniTenant.Id, PatientId = Guid.Parse("aaaaaaa8-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PrescriberId = hscniGp?.Id ?? gpUser.Id,
                MedicationName = "Insulin glargine 100 units/mL solution for injection",
                Route = "Subcutaneous",
                Dosage = "24 units", Frequency = "Once daily at bedtime",
                QuantityIssued = 3, Instructions = "Subcutaneous injection. Rotate injection sites. Store in fridge.",
                Status = PrescriptionStatus.Active, PrescribedDate = DateTime.UtcNow.AddDays(-30),
                SnomedCode = "411529005"
            }
        };

        context.Prescriptions.AddRange(prescriptions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAlertsAsync(ApplicationDbContext context, Tenant nhsTenant)
    {
        var alerts = new List<PatientAlert>
        {
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Title = "Penicillin Allergy", Description = "Patient has confirmed penicillin allergy causing rash. Avoid all penicillin-based antibiotics.",
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa5-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Title = "Aspirin Contraindication", Description = "Patient had GI bleed with Aspirin. Do not prescribe NSAIDs or Aspirin.",
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa5-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Title = "Anticoagulation - Fall Risk", Description = "Patient on Warfarin. High fall risk. Ensure anti-fall precautions in any clinical setting.",
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa7-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Title = "Codeine Allergy", Description = "Codeine causes severe nausea and vomiting. Use alternative analgesia.",
            },
            new() {
                TenantId = nhsTenant.Id, PatientId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Title = "Overdue Diabetic Eye Screen", Description = "Patient has not attended diabetic retinal screening. Referral sent - please ensure attendance.",
            }
        };

        context.PatientAlerts.AddRange(alerts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDepartmentsAsync(ApplicationDbContext context, Tenant hscniTenant)
    {
        if (await context.Departments.AnyAsync())
            return;

        var consultant = await context.Users.FirstOrDefaultAsync(u => u.Email == "consultant.mclaughlin@belfast.hscni.net");

        // Belfast HSCNI Trust departments
        var cardiology = new Department
        {
            Id = Guid.Parse("d0000001-dddd-dddd-dddd-dddddddddddd"),
            TenantId = hscniTenant.Id,
            Name = "Cardiology",
            DepartmentType = DepartmentType.Cardiology,
            Code = "CARD",
            Description = "Specialist cardiac care including interventional cardiology, electrophysiology and heart failure management.",
            Location = "Block A, Level 3",
            PhoneExtension = "3100",
            HeadOfDepartment = "Prof. Aoife McLaughlin",
            IsActive = true
        };

        var neurology = new Department
        {
            Id = Guid.Parse("d0000002-dddd-dddd-dddd-dddddddddddd"),
            TenantId = hscniTenant.Id,
            Name = "Neurology",
            DepartmentType = DepartmentType.Neurology,
            Code = "NEUR",
            Description = "Diagnosis and management of disorders of the nervous system including stroke, epilepsy and multiple sclerosis.",
            Location = "Block B, Level 2",
            PhoneExtension = "3200",
            HeadOfDepartment = "Dr. Conor Hughes",
            IsActive = true
        };

        var oncology = new Department
        {
            Id = Guid.Parse("d0000003-dddd-dddd-dddd-dddddddddddd"),
            TenantId = hscniTenant.Id,
            Name = "Oncology",
            DepartmentType = DepartmentType.Oncology,
            Code = "ONCO",
            Description = "Comprehensive cancer care including medical oncology, chemotherapy and haematological malignancies.",
            Location = "Block C, Level 1",
            PhoneExtension = "3300",
            HeadOfDepartment = "Dr. Mary Brennan",
            IsActive = true
        };

        var orthopaedics = new Department
        {
            Id = Guid.Parse("d0000004-dddd-dddd-dddd-dddddddddddd"),
            TenantId = hscniTenant.Id,
            Name = "Orthopaedics & Trauma",
            DepartmentType = DepartmentType.Orthopaedics,
            Code = "ORTH",
            Description = "Surgical and non-surgical treatment of musculoskeletal conditions, fractures and joint replacements.",
            Location = "Block D, Level 2",
            PhoneExtension = "3400",
            HeadOfDepartment = "Mr. Declan Farrell",
            IsActive = true
        };

        var emergency = new Department
        {
            Id = Guid.Parse("d0000005-dddd-dddd-dddd-dddddddddddd"),
            TenantId = hscniTenant.Id,
            Name = "Emergency & Acute Medicine",
            DepartmentType = DepartmentType.Emergency,
            Code = "A&E",
            Description = "24/7 emergency assessment and treatment of life-threatening and urgent conditions. Major Trauma Centre.",
            Location = "Ground Floor, Main Entrance",
            PhoneExtension = "3500",
            HeadOfDepartment = "Dr. Niall Brady",
            IsActive = true
        };

        var renal = new Department
        {
            Id = Guid.Parse("d0000006-dddd-dddd-dddd-dddddddddddd"),
            TenantId = hscniTenant.Id,
            Name = "Renal Medicine",
            DepartmentType = DepartmentType.Renal,
            Code = "RENAL",
            Description = "Management of acute and chronic kidney disease including dialysis, transplant and CKD clinics.",
            Location = "Block A, Level 1",
            PhoneExtension = "3600",
            HeadOfDepartment = "Dr. Sinéad Kelly",
            IsActive = true
        };

        var respiratory = new Department
        {
            Id = Guid.Parse("d0000007-dddd-dddd-dddd-dddddddddddd"),
            TenantId = hscniTenant.Id,
            Name = "Respiratory Medicine",
            DepartmentType = DepartmentType.Respiratory,
            Code = "RESP",
            Description = "Specialist care for asthma, COPD, lung cancer, sleep disorders and respiratory infections.",
            Location = "Block B, Level 3",
            PhoneExtension = "3700",
            HeadOfDepartment = "Dr. Fionnuala Gallagher",
            IsActive = true
        };

        var icu = new Department
        {
            Id = Guid.Parse("d0000008-dddd-dddd-dddd-dddddddddddd"),
            TenantId = hscniTenant.Id,
            Name = "Intensive Care Unit",
            DepartmentType = DepartmentType.IntensiveCare,
            Code = "ICU",
            Description = "Level 3 critical care providing organ support and intensive monitoring for critically ill patients.",
            Location = "Block A, Level 2",
            PhoneExtension = "3800",
            HeadOfDepartment = "Dr. Brendan Quinlan",
            IsActive = true
        };

        var departments = new List<Department> { cardiology, neurology, oncology, orthopaedics, emergency, renal, respiratory, icu };
        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();

        // Wards
        var wards = new List<Ward>
        {
            // Cardiology wards
            new() { Id = Guid.Parse("e0000001-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = cardiology.Id, Name = "Coronary Care Unit (CCU)", Code = "CCU", BedCount = 12, Location = "Block A, Level 3 North", PhoneExtension = "3110" },
            new() { Id = Guid.Parse("e0000002-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = cardiology.Id, Name = "Cardiology Ward 3A", Code = "3A", BedCount = 28, Location = "Block A, Level 3 South", PhoneExtension = "3120" },
            // Neurology wards
            new() { Id = Guid.Parse("e0000003-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = neurology.Id, Name = "Stroke Unit", Code = "STROKE", BedCount = 20, Location = "Block B, Level 2 East", PhoneExtension = "3210" },
            new() { Id = Guid.Parse("e0000004-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = neurology.Id, Name = "Neurology Ward 2B", Code = "2B", BedCount = 24, Location = "Block B, Level 2 West", PhoneExtension = "3220" },
            // Oncology wards
            new() { Id = Guid.Parse("e0000005-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = oncology.Id, Name = "Macmillan Oncology Ward", Code = "ONCO-W", BedCount = 30, Location = "Block C, Level 1 North", PhoneExtension = "3310" },
            new() { Id = Guid.Parse("e0000006-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = oncology.Id, Name = "Chemotherapy Day Unit", Code = "CHEMO", BedCount = 16, Location = "Block C, Level 1 South", PhoneExtension = "3320" },
            // Orthopaedics wards
            new() { Id = Guid.Parse("e0000007-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = orthopaedics.Id, Name = "Trauma Ward 4C", Code = "4C", BedCount = 32, Location = "Block D, Level 2 North", PhoneExtension = "3410" },
            new() { Id = Guid.Parse("e0000008-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = orthopaedics.Id, Name = "Elective Orthopaedics Ward 4D", Code = "4D", BedCount = 28, Location = "Block D, Level 2 South", PhoneExtension = "3420" },
            // Emergency
            new() { Id = Guid.Parse("e0000009-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = emergency.Id, Name = "Majors Area", Code = "MAJ", BedCount = 20, Location = "Ground Floor East", PhoneExtension = "3510" },
            new() { Id = Guid.Parse("e000000a-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = emergency.Id, Name = "Resuscitation Bay", Code = "RESUS", BedCount = 6, Location = "Ground Floor Centre", PhoneExtension = "3520" },
            // ICU
            new() { Id = Guid.Parse("e000000b-eeee-eeee-eeee-eeeeeeeeeeee"), TenantId = hscniTenant.Id, DepartmentId = icu.Id, Name = "ICU Level 3", Code = "ICU-L3", BedCount = 10, Location = "Block A, Level 2", PhoneExtension = "3810" },
        };

        context.Wards.AddRange(wards);
        await context.SaveChangesAsync();

        // Patient admissions – link HSCNI patients to departments
        var seamusId = Guid.Parse("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaaa");  // Seamus Murphy – cardiology
        var siobhanId = Guid.Parse("aaaaaaa8-aaaa-aaaa-aaaa-aaaaaaaaaaaa"); // Siobhan Doherty – renal (DM1)

        var admissions = new List<PatientAdmission>
        {
            // Seamus Murphy – admitted to Cardiology (AF / HF management)
            new()
            {
                TenantId = hscniTenant.Id,
                PatientId = seamusId,
                DepartmentId = cardiology.Id,
                WardId = Guid.Parse("e0000001-eeee-eeee-eeee-eeeeeeeeeeee"), // CCU
                ConsultantId = consultant?.Id,
                BedNumber = "CCU-4",
                AdmissionType = AdmissionType.Emergency,
                Status = AdmissionStatus.Active,
                AdmissionDate = DateTime.UtcNow.AddDays(-5),
                ExpectedDischargeDate = DateTime.UtcNow.AddDays(3),
                AdmissionReason = "Fast atrial fibrillation with haemodynamic compromise",
                DiagnosisOnAdmission = "Paroxysmal AF with rapid ventricular response. Known heart failure (EF 38%).",
                ReferringGpName = "Dr. Patrick O'Neil",
                ReferringGpOdsCode = "ZT001",
                TriageNotes = "Heart rate 148 bpm, BP 90/60, O2 sats 92% on air. Commenced on rate-control and anticoagulation.",
                TreatmentSummary = "IV bisoprolol, LMWH, echocardiogram arranged. Cardiology registrar review daily."
            },
            // Siobhan Doherty – outpatient nephrology clinic (DM1 CKD)
            new()
            {
                TenantId = hscniTenant.Id,
                PatientId = siobhanId,
                DepartmentId = renal.Id,
                WardId = null,
                ConsultantId = consultant?.Id,
                AdmissionType = AdmissionType.OutPatient,
                Status = AdmissionStatus.Discharged,
                AdmissionDate = DateTime.UtcNow.AddDays(-30),
                DischargeDate = DateTime.UtcNow.AddDays(-30),
                AdmissionReason = "Diabetic nephropathy monitoring – CKD stage 3a",
                DiagnosisOnAdmission = "Type 1 Diabetes with microalbuminuria, eGFR 52.",
                DiagnosisOnDischarge = "CKD stage 3a, recommend ACEi titration, annual renal USS.",
                DischargeNotes = "Ramipril increased to 10mg OD. Repeat bloods in 3 months. Next nephrology appointment 6 months.",
                ReferringGpName = "Dr. Patrick O'Neil"
            },
            // Robert Blackwood (NHS) – previously admitted to Cardiology for AF ablation
            new()
            {
                TenantId = hscniTenant.Id,
                PatientId = Guid.Parse("aaaaaaa5-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                DepartmentId = cardiology.Id,
                WardId = Guid.Parse("e0000002-eeee-eeee-eeee-eeeeeeeeeeee"), // Cardiology Ward 3A
                ConsultantId = consultant?.Id,
                BedNumber = "3A-12",
                AdmissionType = AdmissionType.Elective,
                Status = AdmissionStatus.Discharged,
                AdmissionDate = DateTime.UtcNow.AddMonths(-2),
                DischargeDate = DateTime.UtcNow.AddMonths(-2).AddDays(2),
                AdmissionReason = "Elective cardioversion for persistent AF",
                DiagnosisOnAdmission = "Persistent AF, on anticoagulation.",
                DiagnosisOnDischarge = "Successful DC cardioversion to sinus rhythm. Continue warfarin (target INR 2-3). Bisoprolol continued.",
                DischargeNotes = "Discharged to GP care. Repeat ECG at 4 weeks. Follow-up cardiology OPA in 3 months.",
                ReferringGpName = "Dr. James Smith",
                ReferringGpOdsCode = "A81001"
            }
        };

        context.PatientAdmissions.AddRange(admissions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPatientPortalUsersAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, List<Tenant> tenants)
    {
        // Create demo patient portal accounts linked to existing patient records
        var nhsTenant = tenants.First(t => t.TenantType == TenantType.NhsEngland && t.ClientType == ClientType.GpPractice);

        var portalAccounts = new[]
        {
            // Development/demo portal accounts only — never use these credentials in production.
            // In production, use a password-reset email flow to let patients set their own password.
            (Email: "william.taylor@portal.nhs.uk",   Password: "Patient@2024!",
             FirstName: "William", LastName: "Taylor",
             PatientId: Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), TenantId: nhsTenant.Id),

            (Email: "margaret.hughes@portal.nhs.uk",  Password: "Patient@2024!",
             FirstName: "Margaret", LastName: "Hughes",
             PatientId: Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), TenantId: nhsTenant.Id),
        };

        foreach (var pa in portalAccounts)
        {
            if (await userManager.FindByEmailAsync(pa.Email) != null) continue;

            var user = new ApplicationUser
            {
                UserName = pa.Email, Email = pa.Email,
                FirstName = pa.FirstName, LastName = pa.LastName, Title = "",
                TenantId = pa.TenantId, EmailConfirmed = true, IsActive = true
            };
            var result = await userManager.CreateAsync(user, pa.Password);
            if (!result.Succeeded) continue;

            await userManager.AddToRoleAsync(user, "Patient");

            // Link user to their patient record
            var patient = await context.Patients.FindAsync(pa.PatientId);
            if (patient != null)
            {
                patient.PatientUserId = user.Id;
                context.Patients.Update(patient);
            }
        }

        await context.SaveChangesAsync();
    }
}
