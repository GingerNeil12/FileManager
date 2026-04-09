namespace FileManager.Domain.Models;

public class ApplicationUser
{
    private ApplicationUser(
        Guid id,
        string externalProviderId,
        DateTime createdOn
    )
    {
        Id = id;
        ExternalProviderId = externalProviderId;
        CreatedOn = createdOn;
    }

    public Guid Id { get; }
    public string ExternalProviderId { get; }
    public DateTime CreatedOn { get; }
    
    public virtual IList<FileMetadata> Uploads { get; set; } = [];
    public virtual IList<FileMember> AssignedUploads { get; set; } = [];

    public static ApplicationUser Create(string externalProviderId)
        => new(Guid.NewGuid(), externalProviderId, DateTime.UtcNow);
}