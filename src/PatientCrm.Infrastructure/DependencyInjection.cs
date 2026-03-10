using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatientCrm.Core.Entities;
using PatientCrm.Core.Enums;
using PatientCrm.Core.Interfaces;
using PatientCrm.Infrastructure.Data;
using PatientCrm.Infrastructure.Repositories;

namespace PatientCrm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var providerName = configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=patientcrm.db";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (Enum.TryParse<DatabaseProvider>(providerName, true, out var provider))
            {
                switch (provider)
                {
                    case DatabaseProvider.PostgreSql:
                        options.UseNpgsql(connectionString, b => b.MigrationsAssembly("PatientCrm.Infrastructure"));
                        break;
                    case DatabaseProvider.MySql:
                        options.UseMySQL(connectionString, b => b.MigrationsAssembly("PatientCrm.Infrastructure"));
                        break;
                    case DatabaseProvider.Sqlite:
                        options.UseSqlite(connectionString, b => b.MigrationsAssembly("PatientCrm.Infrastructure"));
                        break;
                    default:
                        options.UseSqlServer(connectionString, b => b.MigrationsAssembly("PatientCrm.Infrastructure"));
                        break;
                }
            }
            else
            {
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("PatientCrm.Infrastructure"));
            }
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPatientRepository, PatientRepository>();

        return services;
    }
}
