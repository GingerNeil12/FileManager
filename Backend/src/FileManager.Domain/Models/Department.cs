namespace FileManager.Domain.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<ApplicationUser> Members { get; set; } = [];
    public virtual ICollection<FileDepartment> AssignedUploads { get; set; } = [];
}