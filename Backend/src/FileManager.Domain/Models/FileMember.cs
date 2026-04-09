namespace FileManager.Domain.Models;

public class FileMember
{
    private FileMetadata? _fileMetadata;
    private ApplicationUser? _assignedTo;

    private FileMember(
        Guid fileMetadataId,
        Guid assignedToId,
        DateTime assignedOn,
        DateTime? downloadedOn,
        DateTime? deletedOn)
    {
        FileMetadataId = fileMetadataId;
        AssignedToId = assignedToId;
        AssignedOn = assignedOn;
        DownloadedOn = downloadedOn;
        DeletedOn = deletedOn;
    }
    
    public Guid FileMetadataId { get; }
    public Guid AssignedToId { get; }
    public DateTime AssignedOn { get; }
    public DateTime? DownloadedOn { get; set; }
    public DateTime? DeletedOn { get; set; }

    public virtual FileMetadata FileMetadata
    {
        set => _fileMetadata = value;
        get => _fileMetadata ?? throw new InvalidOperationException("FileMetadata not initialized.");
    }

    public virtual ApplicationUser AssignedTo
    {
        set => _assignedTo = value;
        get => _assignedTo ?? throw new InvalidOperationException("AssignedTo not initialized.");
    }

    public static FileMember Create(Guid fileMetadataId, Guid assignedToId)
        => new(fileMetadataId, assignedToId, DateTime.UtcNow, null, null);
}