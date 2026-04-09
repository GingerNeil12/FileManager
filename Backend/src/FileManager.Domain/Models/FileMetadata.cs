namespace FileManager.Domain.Models;

public class FileMetadata
{
    private ApplicationUser? _uploadedBy;

    private FileMetadata
    (
        Guid id,
        string location,
        string originalName,
        long size,
        bool isEncrypted,
        Guid uploadedById,
        DateTime uploadedOn,
        DateTime? removedOn
    ) 
    {
        Id = id;
        Location = location;
        OriginalName = originalName;
        Size = size;
        IsEncrypted = isEncrypted;
        UploadedById = uploadedById;
        UploadedOn = uploadedOn;
        RemovedOn = removedOn;
   
    }
    public Guid Id { get; }
    public string Location { get; }
    public string OriginalName { get; }
    public long Size { get; }
    public bool IsEncrypted { get; }
    public Guid UploadedById { get; }
    public DateTime UploadedOn { get;  }
    public DateTime? RemovedOn { get; set; }

    public virtual IList<FileMember> Assignees { get; set; } = [];

    public virtual ApplicationUser UploadedBy
    {
        set => _uploadedBy = value;
        get => _uploadedBy ?? throw new InvalidOperationException("UploadedBy not initialized.");
    }

    public static FileMetadata Create(
        string location,
        string originalName,
        long size,
        bool isEncrypted,
        Guid uploadedById
    )
        => new(Guid.NewGuid(), location, originalName, size, isEncrypted, uploadedById, DateTime.UtcNow, null);
}