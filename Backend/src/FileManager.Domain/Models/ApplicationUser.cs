namespace FileManager.Domain.Models;

public class ApplicationUser
{
    private ApplicationUser(
        Guid id,
        string externalProviderId,
        string email,
        string givenName,
        string familyName,
        int? departmentId,
        DateTime createdOn
    )
    {
        Id = id;
        ExternalProviderId = externalProviderId;
        Email = email;
        GivenName = givenName;
        FamilyName = familyName;
        DepartmentId = departmentId;
        CreatedOn = createdOn;
    }

    public Guid Id { get; }
    public string ExternalProviderId { get; }
    public string Email { get; }
    public string GivenName { get; }
    public string FamilyName { get; }
    public int? DepartmentId { get; }
    public DateTime CreatedOn { get; }

    public virtual Department? Department { get; }
    public virtual ICollection<FileMetadata> Uploads { get; set; } = [];
    public virtual ICollection<FileMember> AssignedUploads { get; set; } = [];

    public static ApplicationUser Create(string externalProviderId, string email, string givenName, string familyName, int? departmentId)
        => new(Guid.NewGuid(), externalProviderId, email, givenName, familyName, departmentId, DateTime.UtcNow);
}