using PatientCrm.Core.Enums;

namespace PatientCrm.Core.Entities;

public class Department : BaseEntity
{
    public required string Name { get; set; }
    public DepartmentType DepartmentType { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? PhoneExtension { get; set; }
    public string? HeadOfDepartment { get; set; }
    public string? HeadConsultantId { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Ward> Wards { get; set; } = new List<Ward>();
    public ICollection<PatientAdmission> Admissions { get; set; } = new List<PatientAdmission>();
}
