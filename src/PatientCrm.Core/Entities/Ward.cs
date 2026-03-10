namespace PatientCrm.Core.Entities;

public class Ward : BaseEntity
{
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }

    public required string Name { get; set; }
    public string? Code { get; set; }
    public int BedCount { get; set; }
    public string? Location { get; set; }
    public string? PhoneExtension { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PatientAdmission> Admissions { get; set; } = new List<PatientAdmission>();
}
