using FileManager.Domain.Common.Enums;

namespace FileManager.Domain.Models;

public class ApplicationUser
{
    private ApplicationUser(
        Guid id,
        string externalProviderId,
        string email,
        string givenName,
        string familyName,
        bool isActive,
        UserRoles role,
        int? departmentId,
        DateTime createdOn
    )
    {
        Id = id;
        ExternalProviderId = externalProviderId;
        Email = email;
        GivenName = givenName;
        FamilyName = familyName;
        IsActive = isActive;
        Role = role;
        DepartmentId = departmentId;
        CreatedOn = createdOn;
    }

    public Guid Id { get; }
    public string ExternalProviderId { get; }
    public string Email { get; private set; }
    public string GivenName { get; private set; }
    public string FamilyName { get; private set; }
    public bool IsActive { get; private set; }
    public UserRoles Role { get; private set; }
    public int? DepartmentId { get; }
    public DateTime CreatedOn { get; }

    public virtual Department? Department { get; }
    public virtual ICollection<FileMetadata> Uploads { get; set; } = [];
    public virtual ICollection<FileMember> AssignedUploads { get; set; } = [];

    public static ApplicationUser Create(
        string externalProviderId,
        string email,
        string givenName,
        string familyName,
        UserRoles role,
        int? departmentId)
        => new(Guid.NewGuid(), externalProviderId, email, givenName, familyName, true, role, departmentId, DateTime.UtcNow);

    public void Disable() => IsActive = false;
    public void Enable() => IsActive = true;
    public void PurgeData()
    {
        const string value = "REDACTED";
        Email = value;
        GivenName = value;
        FamilyName = value;
    }
    public void ChangeRole(UserRoles role) => Role = role;
}