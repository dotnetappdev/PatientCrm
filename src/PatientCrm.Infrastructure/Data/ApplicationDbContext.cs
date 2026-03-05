using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;

namespace PatientCrm.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<ClinicalNote> ClinicalNotes => Set<ClinicalNote>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<MedicalImage> MedicalImages => Set<MedicalImage>();
    public DbSet<PatientAlert> PatientAlerts => Set<PatientAlert>();
    public DbSet<DentalRecord> DentalRecords => Set<DentalRecord>();
    public DbSet<ToothRecord> ToothRecords => Set<ToothRecord>();
    public DbSet<GpRecord> GpRecords => Set<GpRecord>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Tenant
        builder.Entity<Tenant>(e =>
        {
            e.HasIndex(t => t.Slug).IsUnique();
            e.HasIndex(t => t.OdsCode);
            e.Property(t => t.Name).HasMaxLength(200);
            e.Property(t => t.Slug).HasMaxLength(100);
        });

        // Patient
        builder.Entity<Patient>(e =>
        {
            e.HasIndex(p => p.NhsNumber);
            e.HasIndex(p => p.HscniNumber);
            e.HasIndex(p => p.TenantId);
            e.HasQueryFilter(p => !p.IsDeleted);
            e.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            e.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            e.Property(p => p.NhsNumber).HasMaxLength(20);
            e.Property(p => p.HscniNumber).HasMaxLength(20);
            e.Property(p => p.Postcode).HasMaxLength(10);
        });

        // ClinicalNote
        builder.Entity<ClinicalNote>(e =>
        {
            e.HasIndex(n => n.PatientId);
            e.HasIndex(n => n.TenantId);
            e.HasQueryFilter(n => !n.IsDeleted);
            e.HasOne(n => n.Patient)
             .WithMany(p => p.ClinicalNotes)
             .HasForeignKey(n => n.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.Author)
             .WithMany()
             .HasForeignKey(n => n.AuthorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Appointment
        builder.Entity<Appointment>(e =>
        {
            e.HasIndex(a => a.PatientId);
            e.HasIndex(a => a.TenantId);
            e.HasIndex(a => a.StartTime);
            e.HasQueryFilter(a => !a.IsDeleted);
            e.HasOne(a => a.Patient)
             .WithMany(p => p.Appointments)
             .HasForeignKey(a => a.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Provider)
             .WithMany()
             .HasForeignKey(a => a.ProviderId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Prescription
        builder.Entity<Prescription>(e =>
        {
            e.HasIndex(p => p.PatientId);
            e.HasIndex(p => p.TenantId);
            e.HasQueryFilter(p => !p.IsDeleted);
            e.HasOne(p => p.Patient)
             .WithMany(p => p.Prescriptions)
             .HasForeignKey(p => p.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.Prescriber)
             .WithMany()
             .HasForeignKey(p => p.PrescriberId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // MedicalImage
        builder.Entity<MedicalImage>(e =>
        {
            e.HasIndex(i => i.PatientId);
            e.HasIndex(i => i.TenantId);
            e.HasQueryFilter(i => !i.IsDeleted);
            e.HasOne(i => i.Patient)
             .WithMany(p => p.MedicalImages)
             .HasForeignKey(i => i.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.ClinicalNote)
             .WithMany(n => n.Images)
             .HasForeignKey(i => i.ClinicalNoteId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // PatientAlert
        builder.Entity<PatientAlert>(e =>
        {
            e.HasIndex(a => a.PatientId);
            e.HasQueryFilter(a => !a.IsDeleted);
            e.HasOne(a => a.Patient)
             .WithMany(p => p.Alerts)
             .HasForeignKey(a => a.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // DentalRecord
        builder.Entity<DentalRecord>(e =>
        {
            e.HasIndex(d => d.PatientId).IsUnique();
            e.HasQueryFilter(d => !d.IsDeleted);
            e.HasOne(d => d.Patient)
             .WithOne(p => p.DentalRecord)
             .HasForeignKey<DentalRecord>(d => d.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ToothRecord
        builder.Entity<ToothRecord>(e =>
        {
            e.HasIndex(t => t.DentalRecordId);
            e.HasQueryFilter(t => !t.IsDeleted);
            e.HasOne(t => t.DentalRecord)
             .WithMany(d => d.ToothRecords)
             .HasForeignKey(t => t.DentalRecordId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // GpRecord
        builder.Entity<GpRecord>(e =>
        {
            e.HasIndex(g => g.PatientId).IsUnique();
            e.HasQueryFilter(g => !g.IsDeleted);
            e.HasOne(g => g.Patient)
             .WithOne(p => p.GpRecord)
             .HasForeignKey<GpRecord>(g => g.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Referral
        builder.Entity<Referral>(e =>
        {
            e.HasIndex(r => r.GpRecordId);
            e.HasQueryFilter(r => !r.IsDeleted);
            e.HasOne(r => r.GpRecord)
             .WithMany(g => g.Referrals)
             .HasForeignKey(r => r.GpRecordId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Patient)
             .WithMany()
             .HasForeignKey(r => r.PatientId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLog
        builder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.EntityName);
            e.HasIndex(a => a.TenantId);
        });

        // User - Tenant relationship
        builder.Entity<ApplicationUser>(e =>
        {
            e.HasOne(u => u.Tenant)
             .WithMany(t => t.Users)
             .HasForeignKey(u => u.TenantId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
