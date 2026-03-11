namespace PatientCrm.Maui.Models;

public class Department
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DepartmentType DepartmentType { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? PhoneExtension { get; set; }
    public string? HeadOfDepartment { get; set; }
    public bool IsActive { get; set; }
    public List<Ward> Wards { get; set; } = [];
}

public class Ward
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public int BedCount { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
}
